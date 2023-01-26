using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
public class ScreenController : MonoBehaviour
{
    private Rect buttonPos;
    private Image image;
    private Object[] sprites;
    public Text money_display;
    public int regTime;
    public int money;
    private int max_money = 2000;

    void Start()
    {
        Application.runInBackground = true;
        StartCoroutine(MoneyRoutine(regTime));
        BackGroundChange();
    }
    /// <summary>
    /// Cambiar de fondo.
    /// </summary>
    public void BackGroundChange()
    {
        image = gameObject.GetComponent<Image>();
        sprites = Resources.LoadAll("Background", typeof(Sprite));
        Sprite sprite = (Sprite)sprites[Random.Range(0, sprites.Length)];
        image.sprite = sprite;
    }

    void Update()
    {
        if (money >= max_money)
        {
            money = max_money;
        }

        money_display.text = money + "/" + max_money;

    }

   
    void OnGUI()
    {
        
    }
    /// <summary>
    /// Subida de dinero en pantalla 50 dólares por tiempo de parámetro de espera.
    /// </summary>
    /// <param name="regTime"></param>
    /// <returns></returns>
    IEnumerator MoneyRoutine(float regTime)
    {
        while (true)
        {
            if (money + 50 < max_money)
                money += 50;
            else
                money = max_money;
            yield return new WaitForSecondsRealtime(regTime);
            
        }
    }
}