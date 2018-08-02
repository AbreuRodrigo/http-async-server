namespace HttpAsyncServer
{
    internal class Consts
    {
        public const string REGEX_PATH = "^\\{0}$";
        public const string KEY_GENERATOR_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
                
        public const string CONTENT_TYPE_TEXT = "Content-Type";
        public const string CONTENT_LENGTH_TEXT = "Content-Length";
                
        //MIMETYPES
        public const string TEXT_HTML_TYPE = "text/html";
        public const string APPLICATION_JSON_TYPE = "application/json";

        public const string MSG_THERE_ARE_NO_OBJECTS_TO_SHOW = "There are no {0} saved";
        public const string MSG_COULD_NOT_FIND_OBJECT_IN_TABLE = "Could not find object in \'{0}\' table";

        public const string SUCCESS_MSG = "success";
        public const string FAILURE_MSG = "failure";

        public const int HASH_ID_LENGTH = 15;
    }
}