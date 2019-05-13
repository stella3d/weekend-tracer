# weekend-tracer
An implementation of Peter Shirley's [Ray Tracing in One Weekend](http://www.realtimerendering.com/raytracing/Ray%20Tracing%20in%20a%20Weekend.pdf), using C# and Unity's [Burst compiler](https://docs.unity3d.com/Packages/com.unity.burst@1.0/manual/index.html).

### Aknowledgements

Thanks to Peter for the excellent original book.

For a GPU implementation of the book in Unity, see [this excellent post by Jeremy Cowles](https://medium.com/@jcowles/gpu-ray-tracing-in-one-weekend-3e7d874b3b0f)

For another implementation of a path tracer in C# + burst, see [Aras' series](https://aras-p.info/blog/2018/03/28/Daily-Pathtracer-Part-3-CSharp-Unity-Burst/)

All of these were helpful to me in learning.


### Book Window

`Window/Weekend Tracer/Book`

Open this window to find chapter-by-chapter implementations of the book, with a button to draw each of them.

Chapters 1-7 are done as a straight translation of the book, and you can find code for them under `Assets/Scripts/Chapters`.  
Each of these chapters has its own implementation in its own file. 

Chapters 8 until the end of the book re-use the same implementation, found in `Assets/Scripts/BatchedTracer.cs`.

The code that does the actual path tracing will always be found in the job struct's `Execute()` function. 

Code around that is mostly about presenting it in the UI and handling options.


### Interactive Window
`Window/Weekend Tracer/Interactive`

_EXPERIMENTAL, NOT IN THE BOOK_

Try to render a basic Unity scene of just spheres via the raytracing camera, interactively.  
Try opening the scene `ChapterEight` if using this window.



### Miscellaneous

I did a version that parallelized on a per-pixel basis using `IJobParallelFor`, but it had a bug i didn't solve, so i scrapped it.
