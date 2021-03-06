Warning
========
This is still a baby project... I will publish it on nuget soon.

... and create sample projects

... and develop multi-statements calls

... and develop automatic client-side interfaces implementation (will allow to manipulate client side objects that are defined by a common inteface, which implementation only exists server-side)

... and add much more features to come

...soon.

What is it, and what does it do ?
========

> tip : RPC stands for Remote Procedure Call.

RPC# intends to provide an easy way to trigger remote code execution, and remote object handling.
Whatever data provider you use, whatever way you connect to your server (WCF, sockets, HttpClient, ServiceStack...).

This might be compared to .Net Remoting, but much more lightweight, and with more features.

Speaking of which. Here is what it does:

- Complex execution: If *f* and *g* are two remote functions, then executing f(g(x)), or f(x)+g(y) (etc...) will only take one network call
- Async or blocking calls fashion


How do I use it ?
========

Check the samples for more details (soon), but in a nutshell:

#### 1) Implement client-server link

i.e. implement IRpcService or IRpcServiceAsync. Example:

```C#
public class MyService : IRpcService
{
    // assuming you've initialized a WCF connection to your server in this field:
    IMyServer _server;
    
    SerializedEvaluation IRpcService.InvokeRemote(SerializedEvaluation evaluated){
      return _server.InvokeRemote(evaluated);  
    }
    
    IRpcRoot IRpcService.ResolveReference(string reference){
      // get an object by reference...
      // either call a service, get object from cache, or whatever. Your call.
    }
}
```
#### 2) Implement your server hook

his is just a hook that links an inbound request of your communication framework (in this sample : WCF) with RPC#

```C#
SerializedEvaluation IMyServer.InvokeRemote(SerializedEvaluation evaluated){
    return RpcEvaluator.HandleIncomingRequest(evaluated, reference =>
    { 
      // this is a reference resolver.
      // Used to transform client side objects reference to server side objects
        // ... do whatever you have to do this to return
        //   a reference which implements the same interface
        //   as your corresponding client-side object
        return GetMyObjectByReference(reference);
    });
}
```

#### 3) Use RPC# client side !

```C#
MyService service; // your initialized service
IMyObjectA remoteA; IMyObjectB remoteB; // objects that implement IRpcRoot

// this will only take one network call, and execute compositions server-side
var result = service.Call(()=> remoteA.GetSomethin(remoteB.Crap()) + remoteA.ReturnInt());

// call promises (reusable calls) :
var promise = service.CallPromise(()=> remoteA.GetSomethin(remoteB.Crap()) );

var mult = service.Call(()=>promise.Execute() * remoteA.ReturnInt());
var div  = service.Call(()=>promise.Execute() / remoteA.ReturnInt());
```


Other features
========

#### 1) Async/await pattern support

The example above shows how to implement a blocking fashion RPC.

RPC# also supports asynchronous calls: You just have to implement IRpcServiceAsync instead of IRpcService.
Resulting calls are almost the same:

```C#
// Note that in this mode, there is no "CallPromise" method: 
// All calls not awaited are not ran until awaited;
// and thus are, by definition, promises.

var promise = service.Call(()=> remoteA.GetSomethin(remoteB.Crap()) );

// will run TWICE the same server call. Usefull for non pure methods.
await promise;
await promise;

// Force blocking evaluation.
var promiseResult = promise.Execute();

// reuse promise in sub-calls... promise evaluation will happen server side.
var mult = await service.Call(()=>promise.Execute() * remoteA.ReturnInt());
var div  = await service.Call(()=>promise.Execute() / remoteA.ReturnInt());
```