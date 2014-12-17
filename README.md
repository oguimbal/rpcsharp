Warning
========
This is still a baby project... I will publish it on nuget soon.

... and create sample projects

... and add much more features

... and develop automatic client-side interfaces implementation (will allow to manipulate client side objects that are defined by a common inteface, which implementation only exists server-side)

...soon.

What is it, and what does it do ?
========

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
```
