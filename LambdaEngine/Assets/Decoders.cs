namespace LambdaEngine.Assets;

public static class Decoders {
    private static readonly Dictionary<DecoderKey, object> _decoders = new Dictionary<DecoderKey, object>();
        
    public static void Register<T>(string fileType, IDecoder<T> decoder) {
        DecoderKey key = new DecoderKey(fileType, typeof(T));
        
        _decoders.Add(key, decoder);
    }

    public static T Decode<T>(string file) {
        if (!Path.Exists(file)) {
            throw new FileNotFoundException(file);
        }

        string? ext = Path.GetExtension(file);

        if (string.IsNullOrEmpty(ext)) {
            throw new NotSupportedException($"No decoder found for extension: {ext}");
        }
        
        DecoderKey key = new(ext, typeof(T));

        if (!_decoders.TryGetValue(key, out object? decoder)) {
            throw new NotSupportedException($"No decoder found for extension: {ext} and output: {typeof(T)}");
        }

        using (FileStream stream = File.OpenRead(file)) {
            return ((IDecoder<T>)decoder).Decode(stream);   
        }
    }
}