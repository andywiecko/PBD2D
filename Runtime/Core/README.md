# PBD2D.Core

The `Core` assembly contains all contracts/common structs required for entities, components, and systems implementations.

- All components interfaces can be found at [`Contracts/`](Contracts/)
- Structs: primitives, constraints, etc can be found at [`Structs/`](Structs/).
- Extensions, utils, and common jobs can be found at [`Extensions/`](Extensions/), [`Utils/`](Utils/), and [`Jobs/`](Jobs/), respectively.

Below one can find a summary of the most important features.

## At() extension

For fast extracting data from buffers, one can use `At<T,U>` extension, example

```csharp
using var positions = new NativeIndexedArray<Id<Point>, float2>(new float2[]{ 0, 1, 2, 3, 4}, Allocator.Persistent);

var t = (Triangle)(0, 1, 2);
var e = (Edge)(3, 4);

// t0 = (0, 0)
// t1 = (1, 1)
// t2 = (2, 2)
var(t0, t1, t2) = positions.At(t);
// e0 = (3, 3)
// e1 = (4, 4)
var (e0, e1) = positions.At(e);
```

## TriFieldLookup

The struct is used for fast external edge lookup for `TriField`.
For given `Id<Triangle>` and barycentric coordinates `float3`
it returns `Id<ExternalEdge>` which correspond for "the closest" one.

> **Warning**
> 
> `TriFieldLookup` should be initialized before usage!
> Currently, field generation is a brute-force implementation.
> Regeneration during runtime may be costly.


## `a..b` Range extension

Use `..` operator in `foreach` loops, example

```csharp
foreach(var i in 2..10)
{
    // Expected: 2 3 4 5 6 7 8 9 10
    UnityEngine.Debug.Log(i);
}
```

## `RangeMinMax` attribute

The attribute is used for marking serialized data with min/max restrictions, e.g.

```csharp
[SerializeField, RangeMinMax(min: 0, max: 1)]
private float2 value = new(0.25f, 0.75f);
```

Custom property drawer preview can be found below:

![range-min-max-property-drawer](../../Documentation~/range-min-max-property-drawer.png)

Attribute supports for `Vector2` and `float2` types.

## Mouse Interaction Configuration

World's global configuration of the mouse interaction system.
Currently supported quantities:

- radius (size of the area in which points will be grabbed),
- rotation speed (speed of the rotation using mouse scroll).

> **Note**
> 
> Use gizmos for preview the current settings.

## PBD Configuration

World's global configuration of the PBD simulation.
Currently supported quantities:

- steps count (iteration count used in PBD algorithm),
- delta time (time step for each frame),
- global damping (control energy dissipation),
- global external acceleration (by default corresponds to earth gravity acceleration).

## Physical Material

Physical material is used to set up the physical properties of the given entities.
Currently supported quantities:

- friction (controls fraction of the friction force applied during resolving collisions),
- density (areal density of the material, its effect on constraints and collisions).

## PBD2D Jobs Order

Implements [`JobsOrder`](https://github.com/andywiecko/ECS/tree/main/Runtime/Solver/JobsOrder.cs), which allows for proper system configuration within the PBD framework.
Systems execution order is split into 3 categories, which run in order:

- frame start (run once per frame)
- substep (run count configurable via [PBD Configuration](#pbd-configuration))
- frame end (run once per frame)

Below one can see an example `PBD2D Jobs Order` asset

![pbd-jobs-order](../../Documentation~/pbd-jobs-order.png)
