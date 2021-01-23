namespace E.Data
{
    public partial class Cloner
    {
        public void Delete(string target)
        {
            try
            {
                System.Uri targetUri = new System.Uri(target);
                Delete(targetUri);
            }
            catch (System.Exception e)
            {
                ClonerDebug.LogException(e);
            }
        }

        public void Delete(System.Uri target) 
        {
            if(Check(in target, out IStream targetStream))
            {
                commandHandler.AddCommand(() =>
                {
                    void taskAction()
                    {
                        try
                        {
                            if (targetStream.Exists)
                            {
                                if (!targetStream.Delete())
                                    throw new System.IO.IOException("delete target faild.");
                            }
                        }
                        catch (System.Exception e)
                        {
                            ClonerDebug.LogException(e);
                        }
                    }
                    taskHandler.AddTask(taskAction);
                });
            }
        }

        public ClonerRequest Clone(byte[] data, System.Uri target)
        {
            if (Check(in data, in target, out IStream targetStream, out Request request))
            {

            }
            return request;
        }

        public ClonerRequest Clone(string source, string target)
        {
            try
            {
                System.Uri sourceUri = new System.Uri(source);
                System.Uri targetUri = new System.Uri(target);
                return Clone(sourceUri, targetUri);
            }
            catch (System.Exception e)
            {
                ClonerDebug.LogException(e);
                return null;
            }
        }

        public ClonerRequest Clone(string source)
        {
            try
            {
                System.Uri sourceUri = new System.Uri(source);
                return Clone(sourceUri);
            }
            catch (System.Exception e)
            {
                ClonerDebug.LogException(e);
                return null;
            }
        }

        public ClonerRequest Clone(System.Uri source)
        {
            return Clone(source, null);
        }

        /// <summary>
        /// pre read from target and read rest from source 
        /// -> 
        /// write to target if target not null
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public ClonerRequest Clone(System.Uri source, System.Uri target)
        {
            if (Check(in source, in target,
                out IStream sourceStream,
                out IStream targetStream,
                out Request request))
                commandHandler.AddCommand(() =>
                {
                    void commandAction()
                    {
                        byte[] data = null;
                        try
                        {
                            request.IsWorking = true;
                            if (sourceStream != null) sourceStream.Timeout = request.Timeout;
                            if (targetStream != null) targetStream.Timeout = request.Timeout;
                            bool sourceExists = sourceStream != null && sourceStream.Exists;
                            bool targetExists = targetStream != null && targetStream.Exists;
                            //版本 使用lastModified 和 长度 计算而成
                            string sourceVersion = sourceExists ? sourceStream.Version : null;
                            string targetVersion = targetExists ? targetStream.Version : null;
                            //获取不到版本 
                            bool versionEqual = sourceVersion == targetVersion;
                            bool complete = targetExists ? targetStream.Complete : false;
                            
                            if(sourceExists && targetExists && !versionEqual)
                            {
                                if (!(targetStream.Delete() && targetStream.Create()))
                                {
                                    
                                }
                            }
                            if(sourceExists && targetExists && versionEqual)
                            {
                                //c
                            }


                            if (!sourceStream.Exists)
                                throw new System.IO.FileNotFoundException("source does't exist.");
                            if (!sourceStream.CanRead)
                                throw new System.IO.InvalidDataException("source is not allow to read.");
                            long length = sourceStream.Length;
                            if (request.LoadAfterDownloaded)
                            { data = new byte[length]; }
                            long currentPosition = 0;
                            byte[] temp = new byte[CacheSize];
                            if (targetStream != null)
                            {
                                if (targetStream.Exists)
                                {
                                    if (request.LoadAfterDownloaded && targetStream.CanRead)
                                    {
                                        while (currentPosition < targetStream.Length)
                                        {
                                            int readCount = targetStream.Read(temp, 0, CacheSize);
                                            for (int i = 0; i < readCount; i++)
                                            {
                                                data[currentPosition + i] = temp[i];
                                            }
                                            currentPosition += readCount;
                                        }
                                    }
                                    else
                                    { currentPosition = targetStream.Length; }
                                    if (targetStream.Length == length)
                                    { targetStream.Complete = true;  return; }
                                }
                                else
                                {
                                    if (!targetStream.Create())
                                        throw new System.IO.IOException("create file faild.");
                                }
                            }
                            sourceStream.Position = currentPosition;
                            while (currentPosition < length)
                            {
                                int readCount = sourceStream.Read(temp, 0, CacheSize);
                                if (targetStream != null && targetStream.CanWrite)
                                { targetStream.Write(temp, 0, readCount); }
                                if (request.LoadAfterDownloaded)
                                {
                                    for (int i = 0; i < readCount; i++)
                                    {
                                        data[currentPosition + i] = temp[i];
                                    }
                                }
                                currentPosition += readCount;
                            }
                            if (targetStream!= null && targetStream.Length == length)
                            { targetStream.Complete = true; }
                        }
                        catch (System.Exception e)
                        {
                            request.IsError = true;
                            data = null;
                            ClonerDebug.LogException(e);
                        }
                        finally
                        {
                            sourceStream?.Close();
                            targetStream?.Close();
                            request.Data = data;
                            request.Close();
                            commandHandler.AddCommand(() =>
                            {
                                request.onClose?.Invoke();
                            });
                        }
                    }
                    taskHandler.AddTask(commandAction);
                });
            return request;
        }
    }
}
