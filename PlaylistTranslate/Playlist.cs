using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

using Newtonsoft.Json.Linq;

namespace PlaylistTranslate
{
    public class Track
    {
        public static readonly String[] Formats = new[] {
            ".mp3", ".m4a", ".wma", ".flac", ".wav", ".ogg", ".aac"
        };

        public String Title { get; private set; }
        public int Length { get; private set; }
        public String Artist { get; private set; }
        public String Album { get; private set; }

        public String Path { get; set; }

        public Track(JObject obj)
        {
            Title = (String) obj["title"];
            Length = (int) obj["length"];
            Artist = (String) obj["artist"];
            Album = (String) obj["album"];

            Path = null;
        }

        internal bool DiscoverPath(AlbumDirectory[] albums)
        {
            if (albums.Length == 0) return false;

            const float margin = 0.125f;

            var albumLower = Album.ToLower();
            var scores = albums.Select(x => new {
                album = x,
                score = x.Name.Levenshtein(albumLower)
            }).OrderBy(x => x.score).Take(2).ToArray();

            var bestAlbum = scores[0];

            var goodmatch = true;

            var trackFormats = new[] {
                Title.ToLower(),
                String.Format("{0} - {1}", Artist, Title).ToLower()
            };

            if (albums.Length > 1) {
                var next = scores[1];
                if (bestAlbum.score > 1 && (next.score - bestAlbum.score) <= albumLower.Length * margin) {
                    goodmatch = false;
                    bestAlbum = albums.Select(x => new {
                        album = x,
                        score = x.Tracks.Min(y => trackFormats.Min(z => y.Name.Levenshtein(z)))
                    }).OrderBy(x => x.score).First();
                }
            }

            var tracks = bestAlbum.album.Tracks.Select(x => new {
                track = x,
                score = trackFormats.Min(y => x.Name.Levenshtein(y))
            }).OrderBy(x => x.score).Take(2).ToArray();

            var bestTrack = tracks[0];

            if (tracks.Length > 1) {
                var next = tracks[1];
                if ((next.score - bestTrack.score) <= bestTrack.track.Name.Length * margin) {
                    goodmatch = false;
                }
            }

            Path = bestTrack.track.Location;
            return goodmatch;
        }
    }

    public class AlbumDirectory 
    {
        public String Name { get; private set; }
        public String Location { get; private set; }
        public TrackFile[] Tracks { get; private set; }

        public AlbumDirectory(String path)
        {
            Name = Path.GetFileName(path).ToLower();
            Location = path;
            Tracks = Directory.GetFiles(path)
                .Where(x => Track.Formats.Contains(Path.GetExtension(x).ToLower()))
                .Select(x => new TrackFile(x))
                .ToArray();
        }
    }

    public class TrackFile
    {
        public String Name { get; private set; }
        public String Location { get; private set; }

        public TrackFile(String path)
        {
            Name = Path.GetFileNameWithoutExtension(path).ToLower();
            Location = path;
        }
    }

    public class Playlist : IEnumerable<Track>
    {
        public static Playlist Parse(String json)
        {
            return new Playlist(JArray.Parse(json));
        }

	    private Track[] _tracks;

        public int TrackCount { get { return _tracks.Length; } }
        public int TotalLength { get { return _tracks.Sum(x => x.Length); } }

        private Playlist(JArray array)
        {
            _tracks = new Track[array.Count];

            for (var i = 0; i < _tracks.Length; ++i) {
                _tracks[i] = new Track((JObject) array[i]);
            }
        }

        public IEnumerable<Track> DiscoverPaths(IEnumerable<String> roots)
        {
            var albums = roots
                .SelectMany(x => Directory.GetDirectories(x))
                .Select(x => new AlbumDirectory(x))
                .ToArray();

            var unsure = new List<Track>();

            foreach (var track in this) {
                if (!track.DiscoverPath(albums)) {
                    unsure.Add(track);
                }
            }

            return unsure;
        }

        public void Export(String path, Format format)
        {
            using (var stream = File.Create(path)) {
                switch (format) {
                    case Format.XSPF:
                        ExportXSPF(stream);
                        return;
                }
            }
        }

        private void ExportXSPF(Stream stream)
        {
            using (var writer = new StreamWriter(stream)) {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                writer.WriteLine("<playlist version=\"1\" xmlns=\"http://xspf.org/ns/0/\">");
                writer.WriteLine("  <title>Unnamed</title>");
                writer.WriteLine("  <date>{0}</date>", DateTime.UtcNow.ToString("o"));
                writer.WriteLine("  <trackList>");

                foreach (var track in this) {
                    writer.WriteLine("    <track>");
                    writer.WriteLine("      <title>{0}</title>", HttpUtility.HtmlEncode(track.Title));
                    writer.WriteLine("      <creator>{0}</creator>", HttpUtility.HtmlEncode(track.Artist));
                    writer.WriteLine("      <album>{0}</album>", HttpUtility.HtmlEncode(track.Album));
                    writer.WriteLine("      <duration>{0}</duration>", track.Length * 1000);
                    writer.WriteLine("      <location>{0}</location>",
                        HttpUtility.UrlEncode(track.Path)
                            .Replace("+", "%20")
                            .Replace("%2f", "/"));
                    writer.WriteLine("    </track>");
                }

                writer.WriteLine("  </trackList>");
                writer.WriteLine("</playlist>");
            }
        }

        public IEnumerator<Track> GetEnumerator()
        {
            return _tracks.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _tracks.GetEnumerator();
        }
    }
}
