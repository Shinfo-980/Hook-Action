using TMPro;
using UnityEngine;

public class BlinkText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float speed = 1f;

    private void Start()
    {
        if (text == null)
        {
            text = GetComponent<TextMeshProUGUI>();
        }
    }

    private void Update()
    {
        Color color = text.color;
        color.a = (Mathf.Sin(Time.time * speed) + 1f) / 2f;
        text.color = color;
    }
}