// Copyright (C) 2024 Reetus
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using SyncPOEditor;

const int PROJECT_ID = 671847;
Dictionary<string, string> _langCodeFileName = new()
{
    { "zh-Hans", @"..\..\ClassicAssist.Shared\Resources\Strings.zh.resx" },
    { "cs", @"..\..\ClassicAssist.Shared\Resources\Strings.cs.resx" },
    { "it", @"..\..\ClassicAssist.Shared\Resources\Strings.it-IT.resx" },
    { "ko", @"..\..\ClassicAssist.Shared\Resources\Strings.ko-KR.resx" },
    { "pl", @"..\..\ClassicAssist.Shared\Resources\Strings.pl-PL.resx" },
    { "en", @"..\..\ClassicAssist.Shared\Resources\Strings.en-GB.resx" },
    { "en-au", @"..\..\ClassicAssist.Shared\Resources\Strings.en-AU.resx" },
    { "en-us", @"..\..\ClassicAssist.Shared\Resources\Strings.resx" }
};

string? apiToken = Environment.GetEnvironmentVariable( "API_TOKEN" );

ArgumentException.ThrowIfNullOrEmpty( apiToken );

HttpClient client = new();

Dictionary<string, string> resxNeutralTerms = ReadResx( @"..\..\ClassicAssist.Shared\Resources\Strings.resx" );

POEditorResponse<POEditorTerms>? neutralTerms = await GetNeutralTerms();

List<string?> missingTerms = resxNeutralTerms.Keys.Except( ( neutralTerms?.Result?.Terms ?? throw new InvalidOperationException() ).Select( x => x.Term ) ).ToList();

if ( missingTerms.Any() )
{
    POEditorTerm[] terms = missingTerms.Select( x => new POEditorTerm { Term = x } ).ToArray();

    await AddTerms( terms );
}

foreach ( ( string? lang, string? fileName ) in _langCodeFileName )
{
    CultureInfo culture = new( lang );
    Thread.CurrentThread.CurrentCulture = culture;
    Thread.CurrentThread.CurrentUICulture = culture;

    if ( !File.Exists( fileName ) )
    {
        throw new FileNotFoundException( $"File not found: {fileName}" );
    }

    Dictionary<string, string> resxTerms = ReadResx( fileName );

    POEditorResponse<POEditorTerms>? terms = await GetLanguageTerms( lang );

    if ( terms?.Result?.Terms == null )
    {
        continue;
    }

    bool modified = false;

    Dictionary<string, (string, string)> modded = new();

    foreach ( POEditorTerm term in terms.Result.Terms )
    {
        if ( string.IsNullOrEmpty( term.Term ) || string.IsNullOrEmpty( term.Translation?.Content ) )
        {
            continue;
        }

        if ( resxTerms.TryGetValue( term.Term, out string? value ) )
        {
            if ( value.Equals( term.Translation.Content, StringComparison.CurrentCulture ) )
            {
                continue;
            }

            resxTerms[term.Term] = term.Translation.Content;
            modded[term.Term] = ( value, term.Translation.Content );
            modified = true;
        }
        else
        {
            resxTerms.Add( term.Term, term.Translation.Content );
            modded[term.Term] = ( string.Empty, term.Translation.Content );
            modified = true;
        }
    }

    if ( modified )
    {
        foreach ( ( string? key, ( string? oldvalue, string? newvalue ) ) in modded )
        {
            Console.WriteLine( $"Updated: {key} - {oldvalue} -> {newvalue}" );
        }

        WriteResx( fileName, resxTerms );
    }

    Dictionary<string, string> missingTranslations = new();

    foreach ( ( string key, string value ) in resxTerms )
    {
        POEditorTerm? termsRes = terms.Result.Terms.FirstOrDefault( x => x.Term == key );

        if ( termsRes == null || string.IsNullOrEmpty( termsRes.Translation?.Content ) )
        {
            missingTranslations[key] = value;
        }
    }

    if ( missingTranslations.Any() )
    {
        POEditorTerm[] translations = missingTranslations.Select( x => new POEditorTerm { Term = x.Key, Translation = new POEditorTranslation { Content = x.Value } } ).ToArray();
        await AddTranslations( lang, translations );
    }
}

return;

async Task<POEditorResponse<POEditorTerms>?> GetLanguageTerms( string lang )
{
    using HttpResponseMessage response = await client.PostAsync( "https://api.poeditor.com/v2/terms/list", new FormUrlEncodedContent( [
        new KeyValuePair<string, string>( "api_token", apiToken ), new KeyValuePair<string, string>( "id", PROJECT_ID.ToString() ),
        new KeyValuePair<string, string>( "language", lang )
    ] ) );
    response.EnsureSuccessStatusCode();
    string json = await response.Content.ReadAsStringAsync();
    return JsonSerializer.Deserialize<POEditorResponse<POEditorTerms>>( json );
}

async Task<POEditorResponse<POEditorTerms>?> GetNeutralTerms()
{
    using HttpResponseMessage response = await client.PostAsync( "https://api.poeditor.com/v2/terms/list",
        new FormUrlEncodedContent( [new KeyValuePair<string, string>( "api_token", apiToken ), new KeyValuePair<string, string>( "id", PROJECT_ID.ToString() )] ) );
    response.EnsureSuccessStatusCode();
    string json = response.Content.ReadAsStringAsync().Result;
    return JsonSerializer.Deserialize<POEditorResponse<POEditorTerms>>( json );
}

async Task AddTerms( POEditorTerm[] terms )
{
    using HttpResponseMessage response = await client.PostAsync( "https://api.poeditor.com/v2/terms/add", new FormUrlEncodedContent( [
        new KeyValuePair<string, string>( "api_token", apiToken ), new KeyValuePair<string, string>( "id", PROJECT_ID.ToString() ),
        new KeyValuePair<string, string>( "data", JsonSerializer.Serialize( terms ) )
    ] ) );
    response.EnsureSuccessStatusCode();
    await response.Content.ReadAsStringAsync();
}

async Task AddTranslations( string lang, POEditorTerm[] translations )
{
    using HttpResponseMessage response = await client.PostAsync( "https://api.poeditor.com/v2/translations/add", new FormUrlEncodedContent( [
        new KeyValuePair<string, string>( "api_token", apiToken ), new KeyValuePair<string, string>( "id", PROJECT_ID.ToString() ),
        new KeyValuePair<string, string>( "language", lang ), new KeyValuePair<string, string>( "data", JsonSerializer.Serialize( translations ) )
    ] ) );
    response.EnsureSuccessStatusCode();
    await response.Content.ReadAsStringAsync();
}

static Dictionary<string, string> ReadResx( string fileName )
{
    Dictionary<string, string> result = new();

    if ( !File.Exists( fileName ) )
    {
        return result;
    }

    XDocument document = XDocument.Load( fileName );

    foreach ( XElement data in document.Root?.Elements( "data" ) ?? Enumerable.Empty<XElement>() )
    {
        string? key = data.Attribute( "name" )?.Value;
        string? value = data.Element( "value" )?.Value;

        if ( !string.IsNullOrEmpty( key ) && value != null )
        {
            result[key] = value;
        }
    }

    return result;
}

static void WriteResx( string fileName, Dictionary<string, string> values )
{
    XDocument document = File.Exists( fileName ) ? XDocument.Load( fileName ) : new XDocument( new XElement( "root" ) );

    XElement? root = document.Root;

    if ( root == null )
    {
        return;
    }

    root.Elements( "data" ).Remove();

    foreach ( ( string key, string value ) in values )
    {
        root.Add( new XElement( "data", new XAttribute( "name", key ),
            new XAttribute( XNamespace.Xml + "space", "preserve" ), new XElement( "value", value ) ) );
    }

    document.Save( fileName );
}