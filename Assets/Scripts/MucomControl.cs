using System.IO;
using MucomUnity;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MucomControl : MonoBehaviour
{
	public AudioSource _audioSource;
	public InputField _text;
	public Button _compileButton;
	public Button _stopButton;

	private MucomAudioClip _audioClip;
	private MucomMDSound _mdsound;

	private void Awake()
	{
		MucomDotNetUtility.InitializeMucomLogger(true);
		_mdsound = new MucomMDSound(1024, MucomDotNetUtility.OpenFromStreamingAssets);

		_compileButton.OnClickAsObservable().Subscribe(_ =>
		{
			_audioSource.Stop();

			using (var ms = new System.IO.MemoryStream())
			using (var w = new StreamWriter(ms, System.Text.Encoding.GetEncoding(932)))
			{
				w.Write(_text.text);
				w.Flush();
				ms.Seek(0, SeekOrigin.Begin);

				var compiler = MucomDotNetUtility.CreateCompiler();
				var bin = compiler.Compile(ms, MucomDotNetUtility.OpenFromStreamingAssets);

				_audioSource.clip = null;
				_audioClip?.Dispose();
				_audioClip = null;

				_mdsound.YM2608.Reset(0);
				var audioClip = new MucomAudioClip(_mdsound, bin, MucomDotNetUtility.OpenFromStreamingAssets);
				_audioSource.clip = audioClip.UnityAudioClip;
				_audioSource.Play();
			}
		}).AddTo(this);

		_stopButton.OnClickAsObservable().Subscribe(_ =>
		{
			_audioSource.Stop();
		}).AddTo(this);

	}

	private void OnDestroy()
	{
		_audioClip?.Dispose();
		_audioClip = null;
	}
}
