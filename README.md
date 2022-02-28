# ReorderingBehaviorWPF
### Reordering behavior fog WPF controls

Known issues:
> Works only on controls that are children of a Canvas
> Controls are the same size
Will be fixed in future updates

## Dependencies
> Microsoft.Xaml.Behaviors

# Usage
Compile `ReoderBehavor` project to get the dll or take the `Reordering.cs` class and implement it to any of your projects.
## Using in XAML
To use reorderingbehavor in XAML you need to declare namespaces for ReorderingBehavior assembly and xaml.behaviors
```
xmlns:reorder="clr-namespace:ReoderBehavior;assembly=ReoderBehavior"
xmlns:I="http://schemas.microsoft.com/xaml/behaviors"
```
Attach the behavior to a control (must be a child of Canvas)
```
<Rectangle Canvas.Left="250" Canvas.Top="20">
  <I:Interaction.Behaviors>
      <reorder:Reordering/>
  </I:Interaction.Behaviors>
</Rectangle>
```

## Using in code behind
Add `Reordering.cs` to your project or `ReorderingBehavior.dll` assembly to dependencies.
```
using ReoderBehavior;
using Microsoft.Xaml.Behaviors;
```
C# code
```
Button b = new Button();
Reordering reorder = new Reordering();
mainCanvas.Children.Add(b);
reorder.Attach(b);
Interaction.GetBehaviors(b).Add(reorder);
```
## Demo
![reordering gif](https://user-images.githubusercontent.com/20230176/156030242-35b74d08-8565-47b9-8dd3-9863c0d6152c.gif)
