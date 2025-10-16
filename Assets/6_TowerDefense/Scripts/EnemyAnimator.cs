using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.UIElements;
namespace tower.defense {

    [System.Serializable]
    public struct EnemyAnimator {

        const float transitionSpeed = 5f;

        public enum Clip { Move, Intro, Outro, Dying, Appear, Disappear }

        public Clip CurrentClip { get; private set; }


        PlayableGraph graph;
        AnimationMixerPlayable mixer;

        Clip previousClip;
        float transitionProgress;

        bool hasAppearClip, hasDisappearClip;

        public void Configure(Animator animator, EnemyAnimationConfig config) {
            hasAppearClip = config.Appear;
            hasDisappearClip = config.Disappear;

            graph = PlayableGraph.Create();
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            int clipCount = 4;
            if (hasAppearClip) {
                clipCount++;
            }
            if (hasDisappearClip) {
                clipCount++;
            }
            mixer = AnimationMixerPlayable.Create(graph, clipCount);

            AnimationClipPlayable clip = AnimationClipPlayable.Create(graph, config.Move);
            clip.Pause();
            mixer.ConnectInput((int)Clip.Move, clip, 0);

            clip = AnimationClipPlayable.Create(graph, config.Intro);
            clip.SetDuration(config.Intro.length);
            mixer.ConnectInput((int)Clip.Intro, clip, 0);

            clip = AnimationClipPlayable.Create(graph, config.Outro);
            clip.SetDuration(config.Outro.length);
            clip.Pause();
            mixer.ConnectInput((int)Clip.Outro, clip, 0);

            clip = AnimationClipPlayable.Create(graph, config.Dying);
            clip.SetDuration(config.Dying.length);
            clip.Pause();
            mixer.ConnectInput((int)Clip.Dying, clip, 0);

            if (hasAppearClip) {
                clip = AnimationClipPlayable.Create(graph, config.Appear);
                clip.SetDuration(config.Appear.length);
                clip.Pause();
                mixer.ConnectInput((int)Clip.Appear, clip, 0);
            }
            if (hasDisappearClip) {
                clip = AnimationClipPlayable.Create(graph, config.Disappear);
                clip.SetDuration(config.Disappear.length);
                clip.Pause();
                mixer.ConnectInput((int)Clip.Disappear, clip, 0);
            }

            AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "Enemy", animator);
            output.SetSourcePlayable(mixer);
        }

        public bool IsDone => GetPlayable(CurrentClip).IsDone();


        public void GameUpdate() {
            if (transitionProgress >= 0f) {
                transitionProgress += Time.deltaTime * transitionSpeed;
                if (transitionProgress >= 1f) {
                    transitionProgress = -1f;
                    SetWeight(CurrentClip, 1f);
                    SetWeight(previousClip, 0f);
                } else {
                    SetWeight(CurrentClip, transitionProgress);
                    SetWeight(previousClip, 1f - transitionProgress);
                }
            }
        }


        public void PlayIntro() {
            SetWeight(Clip.Intro, 1f);
            CurrentClip = Clip.Intro;
            graph.Play();
            transitionProgress = -1f;

            if (hasAppearClip) {
                GetPlayable(Clip.Appear).Play();
                SetWeight(Clip.Appear, 1f);
            }
        }

        public void PlayMove(float speed) {
            GetPlayable(Clip.Move).SetSpeed(speed);
            BeginTransition(Clip.Move);
            if (hasAppearClip) {
                SetWeight(Clip.Appear, 0f);
            }
        }

        public void PlayOutro() {
            BeginTransition(Clip.Outro);
            if (hasDisappearClip) {
                PlayDisappearFor(Clip.Outro);
            }
        }

        public void PlayDying() {
            BeginTransition(Clip.Dying);
            if (hasDisappearClip) {
                PlayDisappearFor(Clip.Dying);
            }
        }

        void PlayDisappearFor(Clip otherClip) {
            var clip = GetPlayable(Clip.Disappear);
            clip.Play();
            SetWeight(Clip.Disappear, 1f);
        }

        Playable GetPlayable(Clip clip) {
            return mixer.GetInput((int)clip);
        }


        void BeginTransition(Clip nextClip) {
            previousClip = CurrentClip;
            CurrentClip = nextClip;
            transitionProgress = 0f;
            GetPlayable(nextClip).Play();
        }

        void SetWeight(Clip clip, float weight) {
            mixer.SetInputWeight((int)clip, weight);
        }

        public void Stop() {
            graph.Stop();
        }

        public void Destory() {
            graph.Destroy();
        }

    }

    public class DelayPlayableBehaviour : PlayableBehaviour {
        public float delay = 0f;              // 可在 inspector/Clip 编辑
        private double elapsed = 0.0;
        private bool triggered = false;

        // 如果需要在编辑器里配置额外数据，可加 public fields

        public override void OnGraphStart(Playable playable) {
            elapsed = 0.0;
            triggered = false;
        }

        public override void PrepareFrame(Playable playable, FrameData info) {
            if (triggered) return;

            // 累计时间：使用 info.deltaTime 是安全且与 playable 时间一致的方法
            elapsed += info.deltaTime;

            if (elapsed >= delay) {
                triggered = true;
                DoTrigger(playable, info);
            }
        }

        void DoTrigger(Playable playable, FrameData info) {

        }
    }


}