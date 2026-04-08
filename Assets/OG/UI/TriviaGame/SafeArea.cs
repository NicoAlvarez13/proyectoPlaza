using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class SafeArea : VisualElement
{
    public SafeArea()
    {
        // just boring registering callbacks
        if (panel != null)
        {
            panel.visualTree.RegisterCallback<GeometryChangedEvent>(UpdateGeometry);
        }
        else
        {
            RegisterCallback<GeometryChangedEvent>(UpdateGeometry);
        }
    }

    private void UpdateGeometry(GeometryChangedEvent evt)
    {
        // panel will needed to extract proper dimensions
        if (panel == null)
            return;

#if UNITY_EDITOR
        // RuntimePanelUtils.ScreenToPanel are not working with editor's panel
        if (panel.contextType == ContextType.Editor)
        {
            return;
        }
#endif

        Rect safeArea = Screen.safeArea;
        float screenHeight = (float)Screen.height;

        Vector2 safeAreaLeftTop = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(safeArea.xMin, screenHeight - safeArea.yMax));
        Vector2 safeAreaRightBottom = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(Screen.width - safeArea.xMax, safeArea.yMin));

        // setting padding. but you can experiment with margings as well
        style.paddingLeft = safeAreaLeftTop.x;
        style.paddingTop = safeAreaLeftTop.y;
        style.paddingRight = safeAreaRightBottom.x;
        style.paddingBottom = safeAreaRightBottom.y;

        // Find the HeaderFiller element
        VisualElement headerFiller = this.Q<VisualElement>("HeaderFiller");

        if (headerFiller != null)
        {
            // Get the total scaled height of your UI panel
            float totalPanelHeight = panel.visualTree.layout.height;

            // Calculate the percentage of the screen the top safe area takes up
            float heightPercentage = (safeAreaLeftTop.y / totalPanelHeight) * 100f;

            // Apply that exact percentage to the height of the HeaderFiller
            headerFiller.style.height = new Length(heightPercentage, LengthUnit.Percent);

            // Note: Since it is absolute and top: 0, you don't even need percentage. 
            // You could also just apply the panel units directly like this:
            // headerFiller.style.height = safeAreaLeftTop.y;
        }
    }
}