# weekend-tracer
An implementation of Peter Shirley's [Ray Tracing in One Weekend](http://www.realtimerendering.com/raytracing/Ray%20Tracing%20in%20a%20Weekend.pdf), using C# and Unity's Burst compiler.

Thanks to Peter for the awesome book.

**not %100 finished!!**

there is a bug in my implementation of dielectric scatter, in the later chapters.  I'm still trying to figure it out

### Book Window

`Window/Weekend Tracer/Book`

Chapter-by-chapter implementations, with a button to draw them.

Chapters 1-7 are done as a straight translation of the book, and you can find code for them under `Assets/Scripts/Chapters`.
Past that, my code starts to deviate from the book somewhat, but should still be useful.  

There are a few different implementations of the raytracer used in chapters 8 until the end of the book.


### Interactive Window
`Window/Weekend Tracer/Interactive`

_EXPERIMENTAL, NOT IN THE BOOK_

Try to render a basic Unity scene of just spheres via the raytracing camera.  Try opening the scene `ChapterEight`.

