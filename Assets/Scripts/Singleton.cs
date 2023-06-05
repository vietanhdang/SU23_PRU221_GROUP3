using UnityEngine;
/// <summary>
/// Generic Singleton Class
/// Singletons là các lớp chỉ có thể có 1 đối tượng (hoặc thể hiện) tại một thời điểm.
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{

    private static T instance; // Biến lưu trữ thể hiện duy nhất của lớp

    public static T Instance // Thuộc tính lấy thể hiện duy nhất của lớp
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
