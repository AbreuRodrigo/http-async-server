namespace HttpAsyncServer
{
    public class Message
    {
        public static string Success()
        {
            return Consts.SUCCESS_MSG;
        }

        public static string Failure()
        {
            return Consts.FAILURE_MSG;
        }

        public static string NoObjectsFound(string objectsName)
        {
            return string.Format(Consts.MSG_THERE_ARE_NO_OBJECTS_TO_SHOW, objectsName);
        }

        public static string NoObjectFoundById(string table)
        {
            return string.Format(Consts.MSG_COULD_NOT_FIND_OBJECT_IN_TABLE, table);
        }
    }
}