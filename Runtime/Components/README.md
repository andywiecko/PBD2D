# PBD2D.Components

TODO: TOC

The following assembly contains all components implementations provided by the package.
The components shouldn't contain any logic by design.
Components should be treated as pure objects for holding the data and/or configuration.
The given components can be attached to selected entity types only.

## Entity

To define new entity one has to create a class derived from `Entity`

```csharp
public class MyEntity : Entity
{

}
```

## Component

To create new a component it is crucial to add a new component contract at `PBD2D.Core`.
Introducing the contract is essential since system assembly does not know any information about component implementations.

```csharp
public interface IMyComponent : IComponent
{

}
```

Then one has to implement the introduced interface.
`BaseComponent` class can be helpful, however, it is not necessary to use the class and the contract can be implemented on _pure C#_ (non-MonoBehaviour) class.

```csharp
[RequiredComponent(typeof(MyEntity))]
public class MyComponent : BaseComponent, IMyComponent
{

}
```

## Components tuple

Components can be matched into pairs by using tuples.
A component tuple is a virtual component that can be created automatically and does not live on the scene. 
It is useful in the cases when one needs to introduce some kind of interaction e.g. collisions.

```csharp
public IMyTuple : IComponent
{

}
```

```csharp
public MyTuple : ComponentsTuple<IMyComponent, IMyComponent>, IMyTuple
{
     protected override bool InstantiateWhen(IMyComponent c1, IMyComponent c2) => c1.Id != c2.Id;
}
```

Currently, the package supports only two argument tuples.
