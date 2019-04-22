using System.IO.Pipelines;

namespace graphql.server.tests.host
{
    public class MessageRecorder : IDuplexPipe
    {
        private readonly Pipe _readPipe;
        private readonly Pipe _writePipe;

        public MessageRecorder()
        {
            _readPipe = new Pipe();
            _writePipe = new Pipe();
        }

        public PipeReader Input => _readPipe.Reader;

        public PipeWriter Output => _writePipe.Writer;
    }
}