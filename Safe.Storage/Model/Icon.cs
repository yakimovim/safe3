using System;
using System.IO;

namespace EdlinSoftware.Safe.Storage.Model
{
    public class Icon
    {
        private readonly Func<Stream> _streamProvider;

        public Icon(string id, Func<Stream> streamProvider)
        {
            _streamProvider = streamProvider ?? throw new ArgumentNullException(nameof(streamProvider));
            Id = id;
        }

        public string Id { get; }

        public Stream GetStream() => _streamProvider();
    }
}