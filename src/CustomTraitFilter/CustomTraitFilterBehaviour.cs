using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using PebbleKnights.ModLoader;
using UnityEngine;

namespace CustomTraitFilter
{
    public sealed class CustomTraitFilterBehaviour : MonoBehaviour
    {
        private IModContext _context;
        private ManualLogSource _logger;
        private readonly List<TraitEntry> _traits = new List<TraitEntry>();
        private readonly HashSet<string> _disabledTraits = new HashSet<string>();
        private readonly LanguageTextProvider _language = new LanguageTextProvider();
        private Rect _windowRect = new Rect(120f, 90f, 820f, 610f);
        private int _pageIndex;
        private string _search = "";
        private string _status = "Waiting for game database...";
        private bool _visible;
        private float _nextRefreshTime;
        private string _configPath;
        private const float RowHeight = 28f;
        private const int RowsPerPage = 16;

        public void Initialize(IModContext context)
        {
            _context = context;
            _logger = context.Logger;
            _configPath = Path.Combine(context.ModDirectory, "config", "trait-filter.json");
            _language.Initialize(context.GameRootPath, context.Logger);
            _status = _language.Ui("StatusWaiting");
            LoadConfig();
            RefreshData();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
                _visible = !_visible;

            if (_visible)
            {
                if (Input.GetKeyDown(KeyCode.PageUp))
                    ChangePage(-1);
                if (Input.GetKeyDown(KeyCode.PageDown))
                    ChangePage(1);
                if (Input.GetKeyDown(KeyCode.Home))
                    _pageIndex = 0;
                if (Input.GetKeyDown(KeyCode.End))
                    _pageIndex = GetMaxPageIndex();
            }

            if (Input.GetKeyDown(KeyCode.F9))
                RefreshData();

            _language.RefreshIfNeeded(false);

            if (Time.unscaledTime >= _nextRefreshTime && _traits.Count == 0)
            {
                _nextRefreshTime = Time.unscaledTime + 2f;
                RefreshData();
            }
        }

        private void OnGUI()
        {
            if (_context == null)
                return;

            if (!_visible)
            {
                GUI.Label(new Rect(12f, 12f, 360f, 28f), "Trait Filter: F8");
                return;
            }

            _windowRect = GUILayout.Window(842002, _windowRect, DrawWindow, _language.Ui("WindowTitle") + " [" + _language.LanguageCode + "]");
        }

        private void DrawWindow(int id)
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label(_language.Ui("Hotkeys"), GUILayout.Width(170f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(8f);
            GUILayout.BeginHorizontal();
            GUILayout.Label(_language.Ui("Search"), GUILayout.Width(54f));
            var nextSearch = GUILayout.TextField(_search, GUILayout.MinWidth(180f));
            if (nextSearch != _search)
            {
                _search = nextSearch;
                _pageIndex = 0;
            }
            if (GUILayout.Button(_language.Ui("Refresh"), GUILayout.Width(88f)))
                RefreshData();
            if (GUILayout.Button(_language.Ui("Apply"), GUILayout.Width(72f)))
                ApplyAndSave();
            if (GUILayout.Button(_language.Ui("EnableAll"), GUILayout.Width(92f)))
                SetAll(true);
            if (GUILayout.Button(_language.Ui("DisableAll"), GUILayout.Width(96f)))
                SetAll(false);
            GUILayout.EndHorizontal();

            GUILayout.Space(4f);
            GUILayout.Label(_status);
            GUILayout.Label(BuildStateSummary());
            DrawPageControls();

            var listRect = GUILayoutUtility.GetRect(10f, 10000f, 120f, 10000f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            DrawEntries(listRect);

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0f, 0f, 10000f, 28f));
        }

        private void DrawEntries(Rect area)
        {
            if (_traits.Count == 0)
            {
                GUI.Label(area, _language.Ui("NoEntries"));
                return;
            }

            var filteredCount = CountVisible();
            var maxPage = GetMaxPageIndex(filteredCount);
            _pageIndex = Mathf.Clamp(_pageIndex, 0, maxPage);
            var firstVisible = _pageIndex * RowsPerPage;
            var lastVisible = Mathf.Min(filteredCount, firstVisible + RowsPerPage);

            GUI.Box(area, "");

            GUI.BeginGroup(area);
            var rowIndex = 0;
            var visibleIndex = 0;
            for (var i = 0; i < _traits.Count; i++)
            {
                var entry = _traits[i];
                if (!MatchesSearch(entry))
                    continue;

                if (visibleIndex < firstVisible)
                {
                    visibleIndex++;
                    continue;
                }
                if (visibleIndex >= lastVisible)
                    break;

                var y = rowIndex * RowHeight + 4f;
                rowIndex++;
                visibleIndex++;

                var row = new Rect(4f, y, area.width - 8f, RowHeight - 2f);
                GUI.Box(row, "");

                var enabled = IsEntryEnabled(entry);
                var buttonLabel = enabled ? _language.Ui("Enabled") : _language.Ui("Disabled");
                if (GUI.Button(new Rect(row.x + 6f, row.y + 3f, 58f, 22f), buttonLabel))
                    SetEntryEnabled(entry, !enabled);

                GUI.Label(new Rect(row.x + 72f, row.y + 5f, 245f, 20f), entry.Id);
                GUI.Label(new Rect(row.x + 322f, row.y + 5f, Mathf.Max(160f, row.width - 520f), 20f), _language.TraitName(entry));
                GUI.Label(new Rect(row.xMax - 170f, row.y + 5f, 160f, 20f), entry.Category);
            }
            GUI.EndGroup();
        }

        private void DrawPageControls()
        {
            var filteredCount = CountVisible();
            var maxPage = GetMaxPageIndex(filteredCount);
            _pageIndex = Mathf.Clamp(_pageIndex, 0, maxPage);
            var first = filteredCount == 0 ? 0 : _pageIndex * RowsPerPage + 1;
            var last = Mathf.Min(filteredCount, (_pageIndex + 1) * RowsPerPage);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<<", GUILayout.Width(54f)))
                _pageIndex = 0;
            if (GUILayout.Button("<", GUILayout.Width(54f)))
                ChangePage(-1);
            GUILayout.Label(first + "-" + last + " / " + filteredCount + "    Page " + (_pageIndex + 1) + " / " + (maxPage + 1), GUILayout.MinWidth(220f));
            if (GUILayout.Button(">", GUILayout.Width(54f)))
                ChangePage(1);
            if (GUILayout.Button(">>", GUILayout.Width(54f)))
                _pageIndex = maxPage;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private int CountVisible()
        {
            var count = 0;
            for (var i = 0; i < _traits.Count; i++)
            {
                if (MatchesSearch(_traits[i]))
                    count++;
            }

            return count;
        }

        private bool MatchesSearch(TraitEntry entry)
        {
            if (string.IsNullOrEmpty(_search))
                return true;

            return Contains(entry.Id, _search) || Contains(_language.TraitName(entry), _search) || Contains(entry.Category, _search);
        }

        private bool Contains(string value, string search)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            return value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void RefreshData()
        {
            if (_context == null)
                return;

            if (!TraitDataSource.IsDatabaseLoaded())
            {
                _status = _language.Ui("StatusWaiting");
                return;
            }

            _traits.Clear();
            _language.RefreshIfNeeded(true);
            _traits.AddRange(TraitDataSource.LoadTraits());
            ApplyConfigToEntries();
            ApplyAll();
            _pageIndex = Mathf.Clamp(_pageIndex, 0, GetMaxPageIndex());
            _status = "Loaded " + _traits.Count + " traits. Trait filtering applies immediately to loaded data.";

            if (_logger != null)
                _logger.LogInfo(_status);
        }

        private void ChangePage(int delta)
        {
            _pageIndex = Mathf.Clamp(_pageIndex + delta, 0, GetMaxPageIndex());
        }

        private int GetMaxPageIndex()
        {
            return GetMaxPageIndex(CountVisible());
        }

        private int GetMaxPageIndex(int filteredCount)
        {
            if (filteredCount <= 0)
                return 0;

            return Mathf.Max(0, (filteredCount - 1) / RowsPerPage);
        }

        private void LoadConfig()
        {
            var config = ConfigStore.Load(_configPath);
            _disabledTraits.Clear();
            foreach (var id in config.DisabledTraits)
                _disabledTraits.Add(id);
        }

        private void ApplyConfigToEntries()
        {
            for (var i = 0; i < _traits.Count; i++)
                _traits[i].Enabled = !_disabledTraits.Contains(_traits[i].Id);
        }

        private void ApplyAll()
        {
            for (var i = 0; i < _traits.Count; i++)
                _traits[i].Apply();
        }

        private void ApplyAndSave()
        {
            ApplyConfigToEntries();
            ApplyAll();
            SaveConfig();
            _status = _language.Ui("Applied") + ". " + BuildStateSummary();
        }

        private void SetAll(bool enabled)
        {
            _disabledTraits.Clear();
            if (!enabled)
            {
                for (var i = 0; i < _traits.Count; i++)
                    _disabledTraits.Add(_traits[i].Id);
            }

            ApplyConfigToEntries();
            ApplyAll();
            SaveConfig();
            _status = _language.Ui("Applied") + ". " + BuildStateSummary();
        }

        private void SetEntryEnabled(TraitEntry entry, bool enabled)
        {
            if (enabled)
                _disabledTraits.Remove(entry.Id);
            else
                _disabledTraits.Add(entry.Id);

            entry.Enabled = enabled;
            entry.Apply();
            SaveConfig();
            _status = _language.Ui("Applied") + ": " + entry.Id + " = " + (enabled ? _language.Ui("Enabled") : _language.Ui("Disabled"));

            if (_logger != null)
                _logger.LogInfo("Trait filter changed: " + entry.Id + " -> " + (enabled ? "enabled" : "disabled"));
        }

        private bool IsEntryEnabled(TraitEntry entry)
        {
            return !_disabledTraits.Contains(entry.Id);
        }

        private void SaveConfig()
        {
            ConfigStore.Save(_configPath, _disabledTraits);
        }

        private string BuildStateSummary()
        {
            return "Disabled traits: " + _disabledTraits.Count + "/" + _traits.Count;
        }
    }
}
