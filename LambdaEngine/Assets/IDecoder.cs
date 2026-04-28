namespace LambdaEngine.Assets;

public interface IDecoder<T> {
    public T Decode(Stream stream);
}