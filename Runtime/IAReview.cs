using System;
using System.Collections;
using UnityEngine;
using Agraris.Tools.Core;
#if UNITY_ANDROID
using Google.Play.Review;
#endif

namespace Agraris.Tools
{
    public class IAReview : PersistentSingleton<IAReview>
    {
        static IAReview _thisGameObject;
        ReviewManager _reviewManager;
        PlayReviewInfo _playReviewInfo;

        int _playFrequency = 2;
        int _dayFrequency = 2;
        DateTime _lastShowDate;
        static int _currentPlayFrequency;
        bool _success = false;

        bool ReviewWasShownLately => (_lastShowDate.AddDays(_dayFrequency) > DateTime.Today);
        bool CanRequestReview
        {
            get
            {
                if (ReviewWasShownLately) return false;
                if (_currentPlayFrequency > _playFrequency) return true;
                return false;
            }
        }
        static bool EnableLog { get; set; }

        public static void InitReview()
        {
            _thisGameObject = new GameObject("IAReview").AddComponent<IAReview>();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            EnableLog = true;
#endif
        }

        public void ShowReview(int playFrequency, int dayFrequency, ref DateTime lastShowDate)
        {
            _playFrequency = playFrequency;
            _dayFrequency = dayFrequency;
            _lastShowDate = lastShowDate;

            _currentPlayFrequency += 1;

            if (!CanRequestReview)
            {
                if (EnableLog)
                    Debug.LogWarningFormat("Today is {0}. You cannot Request Review until {1}.", DateTime.Today, lastShowDate.AddDays(dayFrequency));

                return;
            }

            StartCoroutine(RequestReviewObject());

            if (_success)
                lastShowDate = DateTime.Today;
        }

        IEnumerator RequestReviewObject()
        {
            if (EnableLog)
                Debug.Log("Requesting Review Object");

#if UNITY_EDITOR
            StartCoroutine(ShowReviewObject());
#endif

            _reviewManager = new ReviewManager();

            var requestFlowOperation = _reviewManager.RequestReviewFlow();
            yield return requestFlowOperation;
            if (requestFlowOperation.Error != ReviewErrorCode.NoError)
            {
                if (EnableLog)
                    Debug.LogError("Requesting Review Object Error" + requestFlowOperation.Error.ToString());

                yield break;
            }

            if (EnableLog)
                Debug.Log("Requesting Review Object Success");
            _playReviewInfo = requestFlowOperation.GetResult();

            StartCoroutine(ShowReviewObject());
        }

        IEnumerator ShowReviewObject()
        {
            if (EnableLog)
                Debug.Log("Showing Review Dialog");

#if UNITY_EDITOR
            _success = true;
            _currentPlayFrequency = 0;
#endif

            if (_reviewManager == null)
                yield break;

            var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
            yield return launchFlowOperation;
            _playReviewInfo = null; // Reset the object
            if (launchFlowOperation.Error != ReviewErrorCode.NoError)
            {
                if (EnableLog)
                    Debug.LogError("Showing Review Object Error. " + launchFlowOperation.Error.ToString());

                _success = false;
                yield break;
            }
            // The flow has finished. The API does not indicate whether the user
            // reviewed or not, or even whether the review dialog was shown. Thus, no
            // matter the result, we continue our app flow.
            if (EnableLog)
                Debug.Log("Showing Review Object Success");

            _success = true;
            _currentPlayFrequency = 0;
        }
    }
}