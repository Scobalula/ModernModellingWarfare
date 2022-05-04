using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhilLibX.Media3D.FileTranslators.CoDXAsset
{
    public abstract class TokenReader : IDisposable
    {
        /// <summary>
        /// Requests the next token from the stream
        /// </summary>
        /// <returns></returns>
        public abstract void Reset();

        /// <summary>
        /// Requests the next token from the stream
        /// </summary>
        /// <returns></returns>
        public abstract TokenData RequestNextToken();

        /// <summary>
        /// Requests the next token from the stream
        /// </summary>
        /// <returns></returns>
        public T RequestNextTokenOfType<T>(string name) where T : TokenData
        {
            while(true)
            {
                var nextToken = RequestNextToken();

                if (nextToken.Token.Name == name && nextToken is T expectedToken)
                    return expectedToken;
                if (nextToken.Token.Name == "//")
                    continue;

                throw new IOException($"Expected token {name} of type: {typeof(T)} but got {nextToken?.Token.Name} of type {nextToken.GetType()}");
            }
        }

        /// <summary>
        /// Requests the next token from the stream
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<TokenData> EnumerateTokens()
        {
            TokenData token;

            Reset();

            while((token = RequestNextToken()) != null)
            {
                yield return token;
            }
        }

        /// <summary>
        /// Finalizes the write, performing any necessary compression, flushing, etc.
        /// </summary>
        public abstract void FinalizeRead();

        /// <summary>
        /// Disposes of the data
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the data
        /// </summary>
        protected abstract void Dispose(bool disposing);
    }
}
