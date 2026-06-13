# Platform Strategy

This project is a public short-video research workflow, not a platform-specific crawler.

## Current model

The downloader reads one URL per line from `urls.txt` and passes those URLs to `yt-dlp`.
If `yt-dlp` supports the site and the video is publicly accessible, the rest of the workflow is the same:

```text
public URL
-> yt-dlp downloads media and metadata
-> FFmpeg extracts audio
-> whisper.cpp transcribes speech
-> build-queue.ps1 creates story cards
```

## Platforms to try

The exact support matrix changes over time with `yt-dlp` and platform rules. Current local extractor checks include:

- Douyin
- TikTok
- X / Twitter
- XiaoHongShu
- Bilibili
- YouTube / YouTube Shorts
- Instagram public videos

## Boundaries

The project should stay conservative:

- Use user-selected public links.
- Keep small batch sizes and sleep intervals.
- Do not bypass login, CAPTCHA, paywalls, private accounts, or access controls.
- Do not promise that every supported platform will work forever.
- Keep downloaded media, models, transcripts, and user link lists out of Git.

## Product direction

The project can evolve from "download one platform" into a creator research pipeline:

- Multi-platform source collection
- Local speech transcription
- Story structure extraction
- Fact and conflict notes
- Original rewriting prompts
- Exportable creator cards

The long-term product is not a downloader. The downloader is only the first step of a content understanding and creation workflow.
