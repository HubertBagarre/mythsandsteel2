using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using LogicUI.FancyTextRendering;

public class PowerPointPlayer : MonoBehaviour
{
    [SerializeField] private GameObject powerPointCanvas, leftArrowGameObject, rightArrowGameObject,imageGameObject,videoGameObject;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Image image;
    [SerializeField] private RawImage rawImage;
    [SerializeField] private Button leftArrowButton,rightArrowButton,endPresentationButton;
    [SerializeField] private MarkdownRenderer titleText, contentText;
    [SerializeField] private RenderTexture videoRenderTexture;

    public ScriptablePowerPoint presentation;

    private void Start()
    {
        endPresentationButton.onClick.AddListener(EndPresentation);
    }


    public void StartPresentation()
    {
        var totalSlides = presentation.slides.Count;
        var currentSlide = 0;
        
        rightArrowButton.onClick.AddListener(GoToNextSlide);
        leftArrowButton.onClick.AddListener(GotToPreviousSlide);
        
        EnableArrows();
        
        powerPointCanvas.SetActive(true);
        
        PlaySlide(presentation.slides[currentSlide]);

        void GoToNextSlide()
        {
            if (currentSlide < totalSlides - 1) currentSlide++;
            PlaySlide(presentation.slides[currentSlide]);
            EnableArrows();
        }

        void GotToPreviousSlide()
        {
            if (currentSlide > 0) currentSlide--;
            PlaySlide(presentation.slides[currentSlide]);
            EnableArrows();
        }

        void EnableArrows()
        {
            leftArrowGameObject.SetActive(currentSlide > 0);
            rightArrowGameObject.SetActive(currentSlide < totalSlides - 1);
        }
        
    }

    private void PlaySlide(ScriptablePowerPoint.Slide slide)
    {
        titleText.Source = slide.title;
        contentText.Source = slide.text;
        
        imageGameObject.SetActive(slide.image != null);
        videoGameObject.SetActive(slide.video != null);
        
        videoPlayer.DOPause();
        
        if (slide.video != null)
        {   
            rawImage.texture = videoRenderTexture;
            videoPlayer.clip = slide.video;
            videoPlayer.DOPlay();
        }
        else
        {
            image.sprite = slide.image;
        }
        
        
    }

    private void EndPresentation()
    {
        powerPointCanvas.SetActive(false);
        
        leftArrowButton.onClick.RemoveAllListeners();
        rightArrowButton.onClick.RemoveAllListeners();
    }
}
