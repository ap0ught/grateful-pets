namespace AiUnity.NLog.Core.Internal.FileAppenders;

internal interface IFileAppenderFactory
{
	BaseFileAppender Open(string fileName, ICreateFileParameters parameters);
}
