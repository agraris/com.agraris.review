using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;

namespace Agraris.Tools
{
    public class IAReviewTest
    {
        DateTime _lastShowDate;

        [UnityTest]
        public IEnumerator CannotShowReviewIfLastShowDateIsToday()
        {
            _lastShowDate = DateTime.Today;

            IAReview.InitReview();

            yield return new WaitForSeconds(.1f);

            Assert.AreEqual(false, IAReview.Instance.ShowReview(0, 1, ref _lastShowDate));
        }

        [UnityTest]
        public IEnumerator CanShowReviewIfLastShowDateIsTodayMinusADay()
        {
            _lastShowDate = DateTime.Today.AddDays(-1);

            IAReview.InitReview();

            yield return new WaitForSeconds(.1f);

            Assert.AreEqual(true, IAReview.Instance.ShowReview(0, 1, ref _lastShowDate));
        }

        [UnityTest]
        public IEnumerator CanShowReviewIfPlayFrequencyIsMatch()
        {
            _lastShowDate = DateTime.Today.AddDays(-1);

            IAReview.InitReview();

            yield return new WaitForSeconds(.1f);

            Assert.AreEqual(false, IAReview.Instance.ShowReview(2, 1, ref _lastShowDate)); // Failed after first play
            Assert.AreEqual(false, IAReview.Instance.ShowReview(2, 1, ref _lastShowDate)); // Failed after second play
            Assert.AreEqual(true, IAReview.Instance.ShowReview(2, 1, ref _lastShowDate)); // Success show review after third play because play frequency is 2
            Assert.AreEqual(false, IAReview.Instance.ShowReview(2, 1, ref _lastShowDate)); // Failed after forth play because _lastShowDate is today
            Assert.AreEqual(false, IAReview.Instance.ShowReview(2, 1, ref _lastShowDate)); // Failed after fifth play because _lastShowDate is today

            _lastShowDate = DateTime.Today.AddDays(-1); // Set last show date to yesterday

            Assert.AreEqual(true, IAReview.Instance.ShowReview(2, 1, ref _lastShowDate)); // Success because _lastShowDate is yesterday
        }
    }
}
