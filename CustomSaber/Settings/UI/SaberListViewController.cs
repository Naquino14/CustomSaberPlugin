using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomSaber.Data;
using CustomSaber.Utilities;
using HMUI;
using IPA.Utilities;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

namespace CustomSaber.Settings.UI
{
    internal class SaberListViewController : BSMLResourceViewController
    {
        public override string ResourceName => "CustomSaber.Settings.UI.Views.saberList.bsml";

        public static SaberListViewController Instance;

        private bool isGeneratingPreview;
        private GameObject preview;

        // Sabers
        private GameObject previewSabers;
        private GameObject leftSaber;
        private GameObject rightSaber;

        // SaberPositions (Local to the previewer)
        private Vector3 sabersPos = new Vector3(0, 0, 0);
        private Vector3 saberLeftPos = new Vector3(0, 0, 0);
        private Vector3 saberRightPos = new Vector3(0, 0.5f, 0);

        public Action<CustomSaberData> customSaberChanged;

        [UIComponent("saberList")]
        public CustomListTableData customListTableData;

        [UIAction("saberSelect")]
        public void Select(TableView _, int row)
        {
            SaberAssetLoader.SelectedSaber = row;
            Configuration.CurrentlySelectedSaber = SaberAssetLoader.CustomSabers[row].FileName;
            customSaberChanged?.Invoke(SaberAssetLoader.CustomSabers[row]);
        }

        [UIAction("reloadSabers")]
        public void ReloadMaterials()
        {
            SaberAssetLoader.Reload();
            SetupList();
            Select(customListTableData.tableView, SaberAssetLoader.SelectedSaber);
        }

        [UIAction("deleteSaber")]
        public void DeleteCurrentSaber()
        {
            var deletedSaber = SaberAssetLoader.DeleteCurrentSaber();

            if (deletedSaber == 0) return;

            SetupList();
            Select(customListTableData.tableView, SaberAssetLoader.SelectedSaber);
        }

        [UIAction("update-confirmation")]
        public void UpdateDeleteConfirmation() => confirmationText.text = $"Are you sure you want to delete\n<color=\"red\">{SaberAssetLoader.CustomSabers[SaberAssetLoader.SelectedSaber].Descriptor.SaberName}</color>?";

        [UIComponent("delete-saber-confirmation-text")]
        public TextMeshProUGUI confirmationText;

        [UIAction("#post-parse")]
        public void SetupList()
        {
            customListTableData.data.Clear();
            foreach (var saber in SaberAssetLoader.CustomSabers)
            {
                var texture = saber.Descriptor.CoverImage?.texture;
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                var customCellInfo = new CustomListTableData.CustomCellInfo(saber.Descriptor.SaberName, saber.Descriptor.AuthorName, sprite);
                customListTableData.data.Add(customCellInfo);
            }

            customListTableData.tableView.ReloadData();
            var selectedSaber = SaberAssetLoader.SelectedSaber;

            customListTableData.tableView.SelectCellWithIdx(selectedSaber);
            if (!customListTableData.tableView.visibleCells.Where(x => x.selected).Any())
                customListTableData.tableView.ScrollToCellWithIdx(selectedSaber, TableViewScroller.ScrollPositionType.Beginning, true);
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);

            Instance = this;


            Select(customListTableData.tableView, SaberAssetLoader.SelectedSaber);
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screen);
        }

        SaberMovementData _leftMovementData = new SaberMovementData();
        SaberMovementData _rightMovementData = new SaberMovementData();
        VRController _leftController;
        VRController _rightController;
        SaberTrailRenderer _trailRendererPrefab;

        private void Update()
        {
            if (_rightController != null)
            {
                Vector3 top = new Vector3(0f, 0f, 1f);
                top = _rightController.rotation * top;
                _rightMovementData.AddNewData(_rightController.transform.position, _rightController.transform.position + top, TimeHelper.time);
            }

            if (_leftController != null)
            {
                Vector3 top = new Vector3(0f, 0f, 1f);
                top = _leftController.rotation * top;
                _leftMovementData.AddNewData(_leftController.transform.position, _leftController.transform.position + top, TimeHelper.time);
            }
        }

        private GameObject InstantiateGameObject(GameObject gameObject, Transform transform = null)
        {
            if (gameObject)
            {
                return transform ? Instantiate(gameObject, transform) : Instantiate(gameObject);
            }

            return null;
        }

        public void ClearHandheldSabers()
        {
            DestroyGameObject(ref leftSaber);
            DestroyGameObject(ref rightSaber);
        }

        float initialSize = -1;
        VRUIControls.VRPointer pointer = null;
        IEnumerator HideOrShowPointer(bool enable = false)
        {
            yield return new WaitUntil(() => pointer = Resources.FindObjectsOfTypeAll<VRUIControls.VRPointer>().FirstOrDefault());
            if (initialSize == -1) initialSize = ReflectionUtil.GetField<float, VRUIControls.VRPointer>(pointer, "_laserPointerWidth");
            pointer.SetField("_laserPointerWidth", enable ? initialSize : 0f);
        }

        public void ShowMenuHandles()
        {
            foreach (var controller in Resources.FindObjectsOfTypeAll<VRController>())
            {
                controller.transform?.Find("MenuHandle")?.gameObject?.SetActive(true);
            }

            StartCoroutine(HideOrShowPointer(true));
        }

        private void DestroyGameObject(ref GameObject gameObject)
        {
            if (gameObject)
            {
                DestroyImmediate(gameObject);
                gameObject = null;
            }
        }
    }
}
