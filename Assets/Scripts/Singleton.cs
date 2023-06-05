using UnityEngine;
/// <summary>
/// Generic Singleton Class
/// Singletons là các lớp chỉ có thể có 1 đối tượng (hoặc thể hiện) tại một thời điểm.
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{

<<<<<<< HEAD
//Generic Singleton Class
//Singletons are classes that can only have 1 object (or instance) at a time.
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
    private static T instance;
    public static T Instance
=======
    private static T instance; // Biến lưu trữ thể hiện duy nhất của lớp

    public static T Instance // Thuộc tính lấy thể hiện duy nhất của lớp
>>>>>>> remotes/origin/main
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>(); // Tìm kiếm thể hiện duy nhất của lớp
                if (instance == null) // Nếu không tìm thấy thì tạo mới
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    instance = singletonObject.AddComponent<T>();
                }
            }
            return instance;
        }
    }
}
