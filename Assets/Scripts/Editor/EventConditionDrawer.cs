//using UnityEditor;
//using UnityEditor.UIElements;
//using UnityEngine;
//using UnityEngine.UIElements;
//using static EventData;
//using static EventData.EventCondition;

//[CustomPropertyDrawer(typeof(EventCondition))]
//public class EventConditionDrawer : PropertyDrawer {
//    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
//        // Create property container element.
//        var container = new VisualElement();

//        var conditionTypeProp = property.FindPropertyRelative("conditionType");
//        var conditionValueProp = property.FindPropertyRelative("conditionValue");

//        var eventConditionProp = property.FindPropertyRelative("eventCondition");

//        // Create property fields.
//        var conditionTypeField = new PropertyField(conditionTypeProp);
//        var componentNameField = new PropertyField(property.FindPropertyRelative("componentName"));
//        var conditionValueField = new PropertyField(conditionValueProp, "Amount");

//        container.Add(conditionTypeField);
//        container.Add(componentNameField);
//        container.Add(conditionValueField);

//        void UpdateVisibility() {
//            var selected = (ConditionType)conditionTypeProp.enumValueIndex;
//            componentNameField.style.display = selected == ConditionType.HasComponent ? DisplayStyle.Flex : DisplayStyle.None;
//            conditionValueField.style.display = (selected == ConditionType.HasComponent || selected == ConditionType.HasParts) ? DisplayStyle.Flex : DisplayStyle.None;
//        }

//        UpdateVisibility();
//        conditionTypeField.RegisterValueChangeCallback(evt => {
//            UpdateVisibility();
//        });

//        return container;
//    }
//}

//using System;
//using System.Linq;
//using UnityEditor;
//using UnityEditor.UIElements;
//using UnityEngine.UIElements;
//using static EventData;

//[CustomPropertyDrawer(typeof(EventConditionInside), true)]
//public class EventConditionInsideDrawer : PropertyDrawer {
//    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
//        var root = new VisualElement();

//        // Get all derived types
//        var types = TypeCache.GetTypesDerivedFrom<EventConditionInside>()
//            .Where(t => !t.IsAbstract)
//            .ToList();

//        var typeNames = types.Select(t => t.Name).ToList();

//        // Dropdown
//        var dropdown = new PopupField<string>("Condition Type", typeNames, 0);
//        root.Add(dropdown);

//        // Container for fields
//        var content = new VisualElement();
//        root.Add(content);

//        void UpdateContent(Type selectedType) {
//            if (property.managedReferenceValue == null ||
//                property.managedReferenceValue.GetType() != selectedType) {
//                property.managedReferenceValue = Activator.CreateInstance(selectedType);
//                property.serializedObject.ApplyModifiedProperties();
//            }

//            content.Clear();

//            var field = new PropertyField(property);
//            field.Bind(property.serializedObject);

//            content.Add(field);
//        }

//        // Initial selection
//        if (property.managedReferenceValue != null) {
//            var currentType = property.managedReferenceValue.GetType();
//            dropdown.index = types.IndexOf(currentType);
//            UpdateContent(currentType);
//        }

//        dropdown.RegisterValueChangedCallback(evt => {
//            var selectedType = types[dropdown.index];
//            UpdateContent(selectedType);
//        });

//        return root;
//    }
//}

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using static EventCondition;

// Not gonna take credit - this was mostly wibe coded.
[CustomPropertyDrawer(typeof(EventCondition))]
public class EventConditionDrawer : PropertyDrawer {
    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        var root = new VisualElement();

        var typeProp = property.FindPropertyRelative("conditionType");
        var conditionProp = property.FindPropertyRelative("eventCondition");

        // Enum field
        var typeField = new PropertyField(typeProp);
        root.Add(typeField);

        // Container for the effect fields
        var container = new VisualElement();
        root.Add(container);

        // Map enum -> type
        Type GetTypeFromEnum(ConditionType type) {
            switch (type) {
                case ConditionType.None: return typeof(NoneCondition);
                case ConditionType.HasComponent: return typeof(HasComponentCondition);
                case ConditionType.HasCurrency: return typeof(HasCurrencyCondition);
                default: return null;
            }
        }

        void UpdateEffect() {
            var selected = (ConditionType)typeProp.enumValueIndex;
            var targetType = GetTypeFromEnum(selected);

            if (targetType == null) {
                conditionProp.managedReferenceValue = null;
            }
            else {
                if (conditionProp.managedReferenceValue == null ||
                    conditionProp.managedReferenceValue.GetType() != targetType) {
                    conditionProp.managedReferenceValue = Activator.CreateInstance(targetType);
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            // Redraw UI
            container.Clear();

            if (conditionProp.managedReferenceValue != null) {
                var field = new PropertyField(conditionProp);
                DrawInline(conditionProp, container);

                //field.Bind(property.serializedObject);
                //container.Add(field);
            }
        }

        // Initial draw
        UpdateEffect();

        // React to enum change
        typeField.RegisterValueChangeCallback(evt => {
            UpdateEffect();
        });

        return root;
    }

    void DrawInline(SerializedProperty prop, VisualElement container) {
        prop.isExpanded = true;

        var iterator = prop.Copy();
        var end = iterator.GetEndProperty();

        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end)) {
            enterChildren = false;

            // Skip the internal managed reference metadata
            if (iterator.propertyPath.EndsWith("managedReferenceFullTypename"))
                continue;

            var field = new PropertyField(iterator.Copy());
            field.Bind(prop.serializedObject);
            container.Add(field);
        }
    }
}