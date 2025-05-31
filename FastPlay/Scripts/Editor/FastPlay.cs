using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace Nexcide.Editor {

    internal class FastPlay {

        private static EditorToolbarToggle _fastPlayButton;
        private static EnterPlayModeOptions _playModeOptionsCached;
        private static bool _restorePlayModeOptions;

        [InitializeOnLoadMethod]
        private static void Initialize() {
            // can't access internal PlayModeButtons class without doing some weird assembly definition bridge stuff, so using Reflection instead
            //PlayModeButtons.onPlayModeButtonsCreated += CreatePlayModeButtons;

            try {
                // get method info for callback method
                Type[] args = new Type[1] { typeof(VisualElement) };
                BindingFlags flags = (BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo callbackMethodInfo = typeof(FastPlay).GetMethod(nameof(AddFastPlayButton), flags, binder: null, args, modifiers: null);

                // get event and create delegate
                Type playModeButtonsClass = ReflectionUtil.GetTypeFromAssembly("UnityEditor.Toolbars.PlayModeButtons");
                EventInfo eventInfo = playModeButtonsClass.GetEvent("onPlayModeButtonsCreated", BindingFlags.NonPublic | BindingFlags.Static);
                Delegate eventHandlerDelegate = Delegate.CreateDelegate(eventInfo.EventHandlerType, firstArgument: null, callbackMethodInfo);

                // get and invoke the add method on the event to register the callback
                MethodInfo addMethodInfo = eventInfo.GetAddMethod(nonPublic: true);
                object[] parameters = new object[] { eventHandlerDelegate };
                addMethodInfo.Invoke(obj: null, flags, binder: null, parameters, culture: null);
            } catch (Exception e) {
                Log.e($"Failed to register with event 'PlayModeButtons.onPlayModeButtonsCreated' via Reflection\n{e}");
            }
        }

        private static void AddFastPlayButton(VisualElement container) {
            // add a space to the toolbar
            VisualElement spacer = new();
            spacer.style.width = 32;
            container.Add(spacer);

            // create and add the fast play button to the toolbar
            _fastPlayButton = new();
            _fastPlayButton.RegisterValueChangedCallback(OnButtonValueChanged);
            UpdateButton(play: true);
            container.Add(_fastPlayButton);

            // register for play mode state changes
            EditorApplication.playModeStateChanged += OnEditorPlayModeStateChanged;
        }

        private static void OnButtonValueChanged(ChangeEvent<bool> changeEvent) {
            if (changeEvent.newValue) {
                _playModeOptionsCached = EditorSettings.enterPlayModeOptions;
                _restorePlayModeOptions = true;
                EditorSettings.enterPlayModeOptions = (EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload);

                EditorApplication.EnterPlaymode();
            } else {
                EditorApplication.ExitPlaymode();
            }
        }

        private static void OnEditorPlayModeStateChanged(PlayModeStateChange playModeStateChange) {
            switch (playModeStateChange) {
                case PlayModeStateChange.ExitingEditMode: {
                    _fastPlayButton.SetValueWithoutNotify(true);
                    UpdateButton(play: false);
                    break;
                }

                case PlayModeStateChange.EnteredPlayMode: {
                    if (_restorePlayModeOptions) {
                        EditorSettings.enterPlayModeOptions = _playModeOptionsCached;
                        _restorePlayModeOptions = false;
                    }

                    _fastPlayButton.SetValueWithoutNotify(true);
                    UpdateButton(play: false);
                    break;
                }

                case PlayModeStateChange.ExitingPlayMode: {
                    _fastPlayButton.SetValueWithoutNotify(false);
                    UpdateButton(play: true);
                    break;
                }
            }
        }

        private static void UpdateButton(bool play) {
            if (play) {
                _fastPlayButton.name = "Fast Play";
                _fastPlayButton.tooltip = "Fast Play";
                _fastPlayButton.icon = Resources.Load<Texture2D>("FastPlayButton");
            } else {
                _fastPlayButton.name = "Stop";
                _fastPlayButton.tooltip = "Stop";
                _fastPlayButton.icon = EditorGUIUtility.IconContent("PlayButton On").image as Texture2D;
            }
        }
    }
}
