using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Xaml.Interactions.Core;
using Avalonia.Xaml.Interactivity;
using ClassicAssist.Shared.UI.Behaviours;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassicAssist.Tests
{
    [TestClass]
    public class XamlBehaviorsRuntimeTests
    {
        private class TestApp : Application
        {
        }

        private static readonly HeadlessUnitTestSession SESSION = HeadlessUnitTestSession.StartNew( typeof( TestApp ) );

        [TestMethod]
        public async Task WillPopulateXamlWithInteractionBehaviors()
        {
            await SESSION.Dispatch( () =>
            {
                const string xaml = """
                                    <Button xmlns="https://github.com/avaloniaui"
                                            xmlns:Interaction="using:Avalonia.Xaml.Interactivity"
                                            xmlns:b="clr-namespace:ClassicAssist.Shared.UI.Behaviours;assembly=ClassicAssist.Shared">
                                      <Interaction.Behaviors>
                                        <b:CloseOnClickBehaviour />
                                      </Interaction.Behaviors>
                                    </Button>
                                    """;

                Button button = AvaloniaRuntimeXamlLoader.Parse<Button>( xaml );

                BehaviorCollection behaviors = Interaction.GetBehaviors( button );

                Assert.AreEqual( 1, behaviors.Count );
                Assert.IsInstanceOfType( behaviors[ 0 ], typeof( CloseOnClickBehaviour ) );
            }, CancellationToken.None );
        }

        [TestMethod]
        public async Task WillPopulateXamlWithEventTriggerAndCommandAction()
        {
            await SESSION.Dispatch( () =>
            {
                const string xaml = """
                                    <StackPanel xmlns="https://github.com/avaloniaui"
                                                xmlns:Interaction="using:Avalonia.Xaml.Interactivity"
                                                xmlns:core="using:Avalonia.Xaml.Interactions.Core">
                                      <Interaction.Behaviors>
                                        <core:EventTriggerBehavior EventName="DoubleTapped">
                                          <core:InvokeCommandAction />
                                        </core:EventTriggerBehavior>
                                      </Interaction.Behaviors>
                                    </StackPanel>
                                    """;

                StackPanel panel = AvaloniaRuntimeXamlLoader.Parse<StackPanel>( xaml );

                BehaviorCollection behaviors = Interaction.GetBehaviors( panel );

                EventTriggerBehavior trigger = behaviors.OfType<EventTriggerBehavior>().FirstOrDefault();

                Assert.IsNotNull( trigger );
                Assert.AreEqual( "DoubleTapped", trigger.EventName );
            }, CancellationToken.None );
        }

        [TestMethod]
        public async Task WillAttachAndDetachBehaviorWithVisualTree()
        {
            await SESSION.Dispatch( () =>
            {
                Window window = new Window();
                Button button = new Button();
                CloseOnClickBehaviour behavior = new CloseOnClickBehaviour();

                Interaction.GetBehaviors( button ).Add( behavior );
                window.Content = button;
                window.Show();

                Assert.AreEqual( button, behavior.AssociatedObject );

                window.Close();

                Assert.IsNull( behavior.AssociatedObject );
            }, CancellationToken.None );
        }
    }
}
