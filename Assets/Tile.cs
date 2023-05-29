using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;


public class Tile : MonoBehaviour , IPointerDownHandler
{
    public Element element;
    private RectTransform rect;
    private Image image;
    public Vector2Int boardPosition = new Vector2Int();
    public Vector2 actualPosition = new Vector2();
    public delegate void TileEvent(Tile tile);
    public static TileEvent OnTileSelected;
    public static TileEvent OnTileDropped;
    private bool isTrackingMouse = false;
    public bool hasBeenChecked = false;
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    public void SetElement(Element el, Sprite visuals)
    {
        element = el;
        image.sprite = visuals;
    }

    public void SetBoardPosition(Vector2Int pos)
    {
        boardPosition = pos;
        gameObject.name = "Tile("+ boardPosition.x +","+ boardPosition.y +")";

        actualPosition = new Vector2(BoardManager.HalfTileSize + boardPosition.x * BoardManager.TileSize,
                                    (BoardManager.HalfTileSize) + boardPosition.y * BoardManager.TileSize);
    }

    public void hilight(Vector2Int dir)
    {
        if(dir == Vector2Int.zero)
        {
            rect.DOScale(1.1f,.1f).SetEase(Ease.OutCirc);
            rect.SetAsLastSibling();
        }
        else if(dir == Vector2Int.down || dir == Vector2Int.up)
        {
            rect.DOScale(new Vector2(0.8f, 1.2f),.25f).SetEase(Ease.OutCirc);
        }
        else // left / right
        {
            rect.DOScale(new Vector2(1.2f, 0.8f),.25f).SetEase(Ease.OutCirc);
        }
    }

    public void shrink()
    {
        rect.DOScale(.8f,.1f).SetEase(Ease.OutCirc);
    }

    public void resetScale()
    {
        rect.DOScale(1f,.1f).SetEase(Ease.InOutCirc);
    }

    public void SnapToPosition(Vector2 pos)
    {
        rect.anchoredPosition = pos;
    }

    public void EaseToPosition(Vector2 pos)
    {
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, pos, .1f);
    }

    public Vector2 GetCurrentActualPosition()
    {
        return rect.anchoredPosition;
    }

    public void MoveToBoardPositon()
    {
        rect.DOAnchorPos(actualPosition, .25f).SetEase(Ease.OutCirc);
    }

    public void SpringToBoardPositon()
    {
        rect.DOAnchorPosX(actualPosition.x, Random.Range(.4f,.6f)).SetEase(Ease.OutElastic);
        rect.DOAnchorPosY(actualPosition.y, Random.Range(.4f,.6f)).SetEase(Ease.OutElastic);
        //rect.DOAnchorPos(actualPosition, .5f).SetEase(Ease.OutElastic);
    }

    public void SnapToBoardPositon()
    {
        rect.anchoredPosition = actualPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(OnTileSelected != null)
        {
            OnTileSelected(this);
        }
    }
}
