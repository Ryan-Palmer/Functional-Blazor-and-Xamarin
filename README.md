# Functional Blazor (+ Xamarin!)

This is a template for Blazor applications which uses a functional reactive style to update the UI.

It has no external dependencies outside of ASP.Net Core and F# (apart from the unit testing project's tools).

The Blazor project is essentially the standard C# Blazor template provided by Visual Studio.
I have just removed the Weather Data page and added a few example controls and behaviours to the Counter page.

## It is NOT a framework!!!

Nobody needs another framework in their life. There is no magic, *no inheritance*, no special attributes. Just a small amount of sensible code that does the job. That's why I called it a template.

You can switch out any and all parts of which there are few - it really is just an example of what I think is a good way of handling state safely and modelling your application flows explicitly.

Run it, mess about with it, break it, bend it to your will. 

## How it works

If you are familiar with Elmish (or any Elm-like system) you will feel right at home as this template pretty much works the same way, in fact the Cmd module is a direct copy and paste from Elmish.

The pattern is more or less MVU, but instead of declarative UI generated from the model we just use Blazor's components and model binding.

There are two main pieces I have added to the template.

### Observable Cache

When you create a normal CLI Event from F# it also implements IObservable out of the box. That means instead of using event.Publish.Add ( or += in C# ) we can call Publish.Subscribe instead, passing a delegate and getting an IDisposable token back which allows us to cancel our subscription without a reference to the event itself.

I wrap one of these events inside an F# MailboxProcessor, which also holds some cached state <'T>. You can post messages to the Mailbox in order to Subscribe to cache changes or Post a new state cache to subscribers.

The real magic comes by registering it as a generic scoped implementation with the DI container. That means you can create a cache of any type just by injecting it, and every request within the scope will get the same instance, allowing them to post to each other.

The implementation of this (along with a stateless version called Messenger) is in the Caching project.

### Program state loop

I won't go into detail about the Elmish style as there is plenty online, but the general idea is that you pass some state and a message to an update function, which returns a new state and potentially further commands to run.

I achieve this behaviour again using an F# Mailbox processor. It receives messages and either handles them directly or delegates them to a child page updater.

It posts the newly generated model to the Razor components using one of the observable caches described above.

It is also set up to print each received message and new model to the debug console so you can see the state transitions. This makes debugging very easy as you can see every change.

In this example project the clock label updates cause it to trigger a print every second, normally it wouldn't be spamming like that.

The implementation of all this is in the Program project and Module.


### Wiring

This is all hooked up in the ASP.Net Core Startup file using a couple of extension methods which call Startup on the Cache, Program and Composition projects, allowing them to register their implementations.

The final step is to compose all of the F# functions and register the Program mailbox which happens in the FunctionRoot module of the Composition project.


### Notes

#### State Scope

Because everything is registered as Scoped, each Blazor Circuit (i.e. user session) gets it's own private Program loop and state. You can check this by looking at the Program model's Id when debugging. 

You could however easily share state between circuits if the scoped Program loops access a shared / global data resource. This could allow for live collaborative working in theory.

#### Server-Side Blazor Considerations

I'm not an expert in Blazor, I only started playing with it this week really. I make no claims as to how you should manage data flow between the server and client and how efficient this system is.

That said however, I think Blazor is supposed to be very efficient at only sending the minimum amount of data to sync the browser DOM and the server's shadow DOM.

One thing you might potentially want to be careful of is stuff like the clock display I have put on the Counter page. This is a Cmd.ofSub subscription set up in the Counter page's Init handler. It pumps a TimeChanged message into the updater every second, which sends a new model update to the UI. In this case it is a tiny text change but I imagine if it was a large update or happened at a fast enough rate you could hit some kind of issue. Maybe it would be fine, I just don't have any data to know. I have installed Application Insights on my own project so that I can keep an eye on the data flow and compute usage. The new live metrics are great to monitor everything during testing.

#### Page update functions

I implement the child page updaters as Mailboxes as well. The reason for this is that I often use them to hold IDisposable tokens from cache or message subscriptions. It allows those tokens to to be held outside the page model and for pages to communicate using messages or stashed data. 

If you don't need that or you don't mind stashing those tokens in the page model you could just use plain old functions. 

If you do want to use the messengers and caches to communicate between pages, there are dispatch functions in the Cache and Message modules to get you started with explanations. If it doesn't make sense raise an issue and I'll demonstrate, it really is simple.

#### UI Updates

The Razor component takes the appropriate part of the Program model and overwrites its current page model, then calls StateHasChanged() to refresh the UI.

The Razor component subscribes to the model updates in OnInitialisedAsync and disposes the subscription token in its Dispose method which is automatically called when the user navigates away from the page.

You can see this in the nested Counter.Razor.cs code file.

UI events are hooked up to methods which send messages to the Program Mailbox. you can see this in the Counter.razor file.

The text box is hooked up one way to send text changes to the update function. If you wanted to also hook a text property in the model up to the text box contents (i.e. a two way binding) you would probably need to debounce it somehow to stop it looping, there are lots of easy ways to achieve that, including the Action extension I have left in the Xamarin Common folder with instructions.

Notice that the model updates are invoked on the render context, they will arrive from the cache on a background thread which cannot update the UI properly.

#### Alerts and Navigation

I use a simple exclusive nav / alert pattern to prevent multiple triggering of these behaviours. Each pending action has a GUID associated with it and I check if it has already been shown, plus the update function only allows one to be set at a time. There are probably better ways to do this, maybe having a list of pending alerts and handling them one at a time etc. It works for demo purposes anyway.

Similarly the navigation pattern is just a quick example of a possible way to handle page routing, I am sure there loads of other cool ways you could implement it.

#### Xamarin and mixing languages

This actually grew out of how I have been writing my Xamarin applications. I follow the same pattern but use MVVMLight for the model state binding. I like using C# on the front end because the tooling is rich and well supported, but F# for
the rest of my applications as, well, it's just awesome! I feel this approach currently offers the best of both worlds.

I have added Xamarin Native projects to the solution which use the same core as the Blazor app to show how easy it is to use the same pattern. Again these are just quickly put together to give you the idea of how it can be set up, nav etc all need to be implemented properly in your favourite way.

They use the same DI as ASP, from Microsoft.Extensions.Hosting, so they can easily hook into the same core as the Blazor app. I create the ServiceProvider in MainApplication / AppDelegate and call Startup the same way as usual.

Both the iOS and Android apps use the same ViewModel. You could easily switch MVVMlight out for James Montemagno's MVVMHelpers or something or even implement INotifyProprtyChanged yourself, it isn't really important.

#### Tests

There are unit tests for most things apart from a few bits of the demo ui as that isn't too important and I just threw some things on the page to show what you can do. The Program and Cache loop is all covered and the Tests project shows a good template for further testing.


