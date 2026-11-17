"""Unity WebGL (Brotli) 빌드를 로컬에서 실행하는 간단한 서버.
사용법: python serve_build.py [빌드폴더] [포트]
"""
import http.server
import os
import sys

root = sys.argv[1] if len(sys.argv) > 1 else r"C:\myfiles\projects\UnityProjects\bloodluckbuild"
port = int(sys.argv[2]) if len(sys.argv) > 2 else 8000


class Handler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *a, **kw):
        super().__init__(*a, directory=root, **kw)

    def end_headers(self):
        path = self.path.split("?")[0]
        if path.endswith(".br"):
            self.send_header("Content-Encoding", "br")
        elif path.endswith(".gz"):
            self.send_header("Content-Encoding", "gzip")
        super().end_headers()

    def guess_type(self, path):
        base = path[:-3] if path.endswith(".br") else path
        if base.endswith(".wasm"):
            return "application/wasm"
        if base.endswith(".js"):
            return "application/javascript"
        if base.endswith(".data"):
            return "application/octet-stream"
        return super().guess_type(path)


print(f"Serving {root} at http://localhost:{port}")
http.server.ThreadingHTTPServer(("localhost", port), Handler).serve_forever()
