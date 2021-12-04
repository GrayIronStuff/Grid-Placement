Grid Placement
-------------------
A project that tests placing walls on a grid, automatic rotation, snapping, and the inability to block off any area.

Place - RMB
Rotate - LMB

The grid size should be relative to the objects width (x).
The offset is mostly for rotation, but it is half the grid size.
 Ex.
 ```
 > width = 10
 > size = 11
 > x and z offset = 11 / 2 = 5.5
 ```

The code is very messy, but you can look it over and see if there is anything you'd like to add to your own project.

Ex.
```
|
V
 
> iiiiii
> i      <-- Has to be an opening
> i    i
> iiiiii

> iiiiiiiiii
>          i
>          i
>    iiiiiii
>    i
>    i
>   ii
> ^
> Has to be an opening
```
