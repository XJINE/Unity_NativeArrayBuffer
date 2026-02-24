# Unity_NativeArrayBuffer

Buffering native arrays for async processes.

## Importing

You can use Package Manager or import it directly.

```
https://github.com/XJINE/Unity_NativeArrayBuffer.git?path=Assets/Packages/NativeArrayBuffers
```

## How to use

```
private NativeArrayBuffer<T> _Tbuffers;
~
var nativeArrayRef = _Tbuffers.GetRef(out var bufferIndex);

if (bufferIndex != -1)
{
	var sizeBytes = ~;

	AsyncGPUReadback.RequestIntoNativeArray(ref nativeArrayRef, ~Buffer, sizeBytes, 0,
	(request) =>
	{
		// Sometimes NativeArrayBuffer is already disposed when the callback is invoked.
		if (request.hasError || _Tbuffers == null)
		{
			return;
		}

		~_Tbuffers[bufferIndex];~
		~_Tbuffers.Release(bufferIndex);
	});
}
```