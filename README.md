# weekend-tracer
An implementation of Peter Shirley's [Ray Tracing in One Weekend](http://www.realtimerendering.com/raytracing/Ray%20Tracing%20in%20a%20Weekend.pdf), using C# and Unity's Burst compiler.

### Aknowledgements

Thanks to Peter for the excellent original book.

For a GPU implementation of the book in Unity, see [this excellent post by Jeremy Cowles](https://medium.com/@jcowles/gpu-ray-tracing-in-one-weekend-3e7d874b3b0f)

For another implementation of a path tracer in C# + burst, see [Aras' series](https://aras-p.info/blog/2018/03/28/Daily-Pathtracer-Part-3-CSharp-Unity-Burst/)

All of these were helpful to me in learning.


### Book Window

`Window/Weekend Tracer/Book`

Chapter-by-chapter implementations, with a button to draw them.

Chapters 1-7 are done as a straight translation of the book, and you can find code for them under `Assets/Scripts/Chapters`.  
Each of these chapters has its own implementation in its own file.  

Chapters 8 until the end of the book re-use the same implementation, found in `Assets/Scripts/BatchedTracer.cs`.


### Interactive Window
`Window/Weekend Tracer/Interactive`

_EXPERIMENTAL, NOT IN THE BOOK_

Try to render a basic Unity scene of just spheres via the raytracing camera, interactively.  
Try opening the scene `ChapterEight` if using this window.

