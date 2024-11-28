using System;
using System.Globalization;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Viroo.SceneLoader.SceneContext;
using Virtualware.Localization;

namespace VirooLab
{
    public class ChangeLanguage : MonoBehaviour
    {
        private ISceneLocalizationService sceneLocalizationService;
        private ILocalizationService localizationService;

        private bool injectionDone;

        protected void Inject(ISceneLocalizationService sceneLocalizationService, ILocalizationService localizationService)
        {
            this.sceneLocalizationService = sceneLocalizationService;
            this.localizationService = localizationService;

            injectionDone = true;
        }

        protected void Awake()
        {
            this.QueueForInject();
        }

#pragma warning disable S3168 // "async" methods should not return "void"
        public async void SetLanguage(string locale)
#pragma warning restore S3168 // "async" methods should not return "void"
        {
            await UniTask.WaitUntil(() => injectionDone, cancellationToken: destroyCancellationToken);

            //THIS IS TO CHANGE LANGUAGE OF THE SCENE
            CultureInfo sceneCultureInfo = sceneLocalizationService.AvailableCultures
                .FirstOrDefault(c => c.Name.Equals(locale, StringComparison.Ordinal));
            if (sceneCultureInfo != null)
            {
                sceneLocalizationService.CurrentCulture = sceneCultureInfo;
            }

            //THIS IS TO CHANGE LANGUAGE OF THE VIROO CORE 
            CultureInfo cultureInfo = localizationService.AvailableCultures
                .FirstOrDefault(c => c.Name.Equals(locale, StringComparison.Ordinal));
            if (cultureInfo != null)
            {
                localizationService.CurrentCulture = cultureInfo;
            }
        }
    }
}
