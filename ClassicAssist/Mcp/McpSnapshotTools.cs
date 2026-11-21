using System;
using System.Collections.Generic;



using System.IO;
using Assistant;
using ClassicAssist.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using static ClassicAssist.Misc.NativeMethods;

namespace ClassicAssist.Mcp
{
    public static class McpSnapshotTools
    {
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        public static IReadOnlyList<McpTool> GetTools()
        {
            return new List<McpTool>
            {
                new McpTool
                {
                    Name = "getSnapshot",
                    Description =
                        "Capture the game client (or the whole desktop with fullscreen:true) and return it as an image the " +
                        "model can see, plus basic metadata. The capture is kept in memory and returned as base64 - no file is written.",
                    InputSchema = McpTools.ObjectSchema(
                        new JObject
                        {
                            ["fullscreen"] = new JObject
                            {
                                ["type"] = "boolean",
                                ["description"] = "Capture the entire desktop instead of the game client window (default false)."
                            },
                            ["maxWidth"] = McpTools.IntegerProperty(
                                "Optional maximum image width; wider captures are downscaled (default 0 = native)." ),
                            ["format"] = new JObject
                            {
                                ["type"] = "string",
                                ["enum"] = new JArray( "png", "jpeg" ),
                                ["description"] = "Image format: png (default) or jpeg (smaller)."
                            }
                        } )
                }
            };
        }

        public static CallToolResult Invoke( string name, JObject args )
        {
            try
            {
                switch ( name )
                {
                    case "getSnapshot":
                        return GetSnapshot( McpTools.GetBool( args, "fullscreen" ), McpTools.GetInt( args, "maxWidth" ),
                            McpTools.GetString( args, "format" ) );
                    default:
                        return null;
                }
            }
            catch ( Exception e )
            {
                return McpTools.Error( e.Message );
            }
        }

        private static CallToolResult GetSnapshot( bool fullscreen, int? maxWidth, string format )
        {
            SKEncodedImageFormat imageFormat;
            string mimeType;
            string formatName;

            switch ( format?.ToLowerInvariant() )
            {
                case "jpeg":
                case "jpg":
                    imageFormat = SKEncodedImageFormat.Jpeg;
                    mimeType = "image/jpeg";
                    formatName = "jpeg";
                    break;
                default:
                    imageFormat = SKEncodedImageFormat.Png;
                    mimeType = "image/png";
                    formatName = "png";
                    break;
            }

            byte[] bytes;
            int width;
            int height;

            using ( SKBitmap bitmap = Capture( fullscreen, maxWidth ) )
            {
                // Report the returned (possibly downscaled) dimensions, not the original capture size
                width = bitmap.Width;
                height = bitmap.Height;

                using ( SKData data = bitmap.Encode( imageFormat, 100 ) )
                {
                    bytes = data.ToArray();
                }
            }

            return new CallToolResult
            {
                Content = new List<McpContent>
                {
                    new McpContent { Type = "image", Data = Convert.ToBase64String( bytes ), MimeType = mimeType },
                    new McpContent
                    {
                        Type = "text",
                        Text = new JObject
                        {
                            ["fullscreen"] = fullscreen,
                            ["width"] = width,
                            ["height"] = height,
                            ["format"] = formatName,
                            ["capturedAt"] = DateTime.Now.ToString( "o" ),
                            ["byteLength"] = bytes.Length
                        }.ToString( Formatting.Indented )
                    }
                }
            };
        }

        private static SKBitmap Capture( bool fullscreen, int? maxWidth )
        {
            int x = 0, y = 0, width, height;

            if ( fullscreen )
            {
                width = GetSystemMetrics( SM_CXSCREEN );
                height = GetSystemMetrics( SM_CYSCREEN );
            }
            else
            {
                IntPtr window = Engine.WindowHandle;

                if ( window == IntPtr.Zero )
                {
                    throw new InvalidOperationException( "Game window handle is not available." );
                }

                GetClientRect( window, out RECT rect );
                width = rect.Right - rect.Left;
                height = rect.Bottom - rect.Top;

                POINT origin = default;

                if ( !ClientToScreen( window, ref origin ) )
                {
                    throw new InvalidOperationException( "Unable to determine game window position." );
                }

                x = origin.X;
                y = origin.Y;
            }

            if ( width <= 0 || height <= 0 )
            {
                throw new InvalidOperationException( "Captured region is empty." );
            }

            SKBitmap bitmap = ScreenCapture.CaptureRectangle( x, y, width, height );

            if ( bitmap == null )
            {
                throw new InvalidOperationException( "Screen capture is not supported on this platform." );
            }

            if ( maxWidth != null && maxWidth.Value > 0 && width > maxWidth.Value )
            {
                using ( bitmap )
                {
                    return Resize( bitmap, maxWidth.Value );
                }
            }

            return bitmap;
        }

        private static SKBitmap Resize( SKBitmap source, int maxWidth )
        {
            int width = maxWidth;
            int height = Math.Max( 1, (int) Math.Round( source.Height * ( width / (double) source.Width ) ) );

            SKBitmap resized = new SKBitmap( width, height );

            using ( SKCanvas canvas = new SKCanvas( resized ) )
            using ( SKPaint paint = new SKPaint { FilterQuality = SKFilterQuality.High } )
            {
                canvas.DrawBitmap( source, new SKRect( 0, 0, width, height ), paint );
            }

            return resized;
        }
    }
}
