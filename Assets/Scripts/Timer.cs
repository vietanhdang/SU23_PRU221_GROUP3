using UnityEngine;

/// <summary>
/// Lớp này dùng để đếm thời gian
/// </summary>
public class Timer : MonoBehaviour
{
    #region Fields

    float totalSeconds = 0; // Tổng số giây

    float elapsedSeconds = 0; // Số giây đã trôi qua
    bool running = false; // Đang chạy hay không
    bool started = false; // Đã bắt đầu hay chưa

    #endregion

    #region Properties

    /// <summary>
    /// Sets the duration of the timer
    /// The duration can only be set if the timer isn't currently running
    /// </summary>
    /// <value>duration</value>
    public float Duration
    {
        set
        {
            if (!running)
            {
                totalSeconds = value;
            }
        }
    }

    /// <summary>
    /// Gets whether or not the timer has finished running
    /// This property returns false if the timer has never been started
    /// </summary>
    /// <value>true if finished; otherwise, false.</value>
    public bool Finished
    {
        get { return started && !running; }
    }

    /// <summary>
    /// Gets whether or not the timer is currently running
    /// </summary>
    /// <value>true if running; otherwise, false.</value>
    public bool Running
    {
        get { return running; }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        if (running)
        {
            elapsedSeconds += Time.deltaTime;
            if (elapsedSeconds >= totalSeconds)
            {
                running = false;
            }
        }
    }

    /// <summary>
    /// Chạy đếm thời gian
    /// Vì một đếm thời gian có thời gian bằng 0 không có ý nghĩa,
    /// đếm thời gian chỉ chạy khi tổng số giây lớn hơn 0
    /// Điều này cũng đảm bảo người sử dụng lớp đã thiết lập thời lượng thành một giá trị lớn hơn 0
    /// </summary>
    public void Run()
    {
        // only run with valid duration
        if (totalSeconds > 0)
        {
            started = true;
            running = true;
            elapsedSeconds = 0;
        }
    }

    #endregion
}
