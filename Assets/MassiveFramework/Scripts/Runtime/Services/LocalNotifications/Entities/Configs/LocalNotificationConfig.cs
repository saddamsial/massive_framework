﻿using System;
using System.Globalization;
using UnityEngine;

namespace MassiveCore.Framework.Runtime
{
    [CreateAssetMenu(fileName = "local_notification_config", menuName = "Massive Framework/Configs/Local Notification Config")]
    public class LocalNotificationConfig : ScriptableObject
    {
        [SerializeField]
        private string _title;

        [SerializeField]
        private string _text;

        [SerializeField]
        private string _time = "12:00";

        public virtual string Title => _title;
        public virtual string Text => _text;
        public TimeSpan TimeSpan => TimeSpan.ParseExact(_time, "hh\\:mm", CultureInfo.InvariantCulture);
    }
}
