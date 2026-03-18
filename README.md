# Unity_NativeArrayBuffer

Buffering native arrays for async processes.

## Importing

You can use Package Manager or import it directly.

```
https://github.com/XJINE/Unity_NativeArrayBuffer.git?path=Assets/Packages/NativeArrayBuffers
```

## How to use

```
private NativeArrayBuffer<T>    _Tbuffers;
private AsyncGPUReadbackRequest _request;
~

if (_Tbuffers.TryRent(out var bufferHandle))
{
	var sizeBytes = ~;

	_request = AsyncGPUReadback.RequestIntoNativeArray
	(ref bufferHandle.Array, ~Buffer, sizeBytes, 0, (request) =>
	{
		if (request.hasError)
		{
			return;
		}

		~use bufferHandle.Array~
		bufferHandle.Dispose();
	});
}

void OnDestroy()
{
	// Once the last request is finished, all requests are complete.
	_request.WaitForCompletion();
	_Tbuffers?.Dispose();
}
```