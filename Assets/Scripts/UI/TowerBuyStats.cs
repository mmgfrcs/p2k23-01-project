﻿using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdInfinitum.UI
{
    public class TowerBuyStats : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI statName;
        [SerializeField] private TextMeshProUGUI statValue;
        [SerializeField] private Image statIcon;

        private float _value;
        private string _unit;
        private bool _isDecimal;
        private float _upgrade;

        private void SetValueText()
        {
            StringBuilder sb = new StringBuilder();

            if (_isDecimal) sb.AppendFormat("{0:N1} ", _value);
            else sb.AppendFormat("{0:N0} ", _value);

            if (_upgrade > 0)
            {
                if (_isDecimal) sb.AppendFormat("<color=green>+{0:N1}</color>", _upgrade);
                else sb.AppendFormat("<color=green>+{0:N0}</color> ", _upgrade);
            }
            else sb.Append(_unit);

            statValue.text = sb.ToString();
        }

        public void SetStat(string name, float value, string unit, bool isDecimal, Sprite icon)
        {
            statName.text = name;
            _value = value;
            _unit = unit;
            _isDecimal = isDecimal;
            _upgrade = 0;
            statIcon.sprite = icon;
            SetValueText();
        }

        public void SetStatString(string name, string value, Sprite icon)
        {
            statName.text = name;
            statValue.text = value;
            _value = 0;
            _unit = "";
            _isDecimal = false;
            _upgrade = 0;
            statIcon.sprite = icon;
        }

        public void SetUpgrade(float upgrade)
        {
            _upgrade = upgrade;
            SetValueText();
        }
    }
}