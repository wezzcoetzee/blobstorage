using System.Runtime.Serialization;

namespace blobstorage.Exceptions;

[Serializable]
public class BlobClientNotFoundException : Exception
{
    public BlobClientNotFoundException() : base("Azure blob client not found")
    { }

    protected BlobClientNotFoundException(SerializationInfo info,
        StreamingContext context) : base(info, context)
    { }
}

[Serializable]
public class BlobContainerNotFoundException : Exception
{
    public BlobContainerNotFoundException() : base("Azure blob container not found")
    { }

    protected BlobContainerNotFoundException(SerializationInfo info,
        StreamingContext context) : base(info, context)
    { }
}

[Serializable]
public class BlobAlreadyExistsException : Exception
{
    public BlobAlreadyExistsException() : base("There is already a blob with this name")
    { }

    protected BlobAlreadyExistsException(SerializationInfo info,
        StreamingContext context) : base(info, context)
    { }
}
