#!/usr/bin/env bash
# Проверка эталонных решений (для автора курса). См. комментарии в verify-solutions.ps1.
# Запуск из корня курса: bash tools/verify-solutions.sh [NN]
set -u
root="$(cd "$(dirname "$0")/.." && pwd)"
verify="$root/.verify"
only="${1:-}"
failed=()
mkdir -p "$verify"
cp "$root/Directory.Build.props" "$verify/"
for dir in "$root"/modules/*/; do
  name="$(basename "$dir")"; nn="${name:0:2}"
  [[ -n "$only" && "$nn" != "$only" ]] && continue
  [[ -d "$dir/starter" && -d "$root/.teacher/solutions/$nn" ]] || continue
  echo "=== Module $nn ==="
  rm -rf "$verify/$nn"; mkdir -p "$verify/$nn"
  cp -r "$dir/starter/." "$verify/$nn/"
  cp -r "$root/.teacher/solutions/$nn/." "$verify/$nn/"   # эталон повторяет структуру starter/
  (cd "$verify/$nn" && dotnet test --nologo) || failed+=("$nn")
done
if (( ${#failed[@]} )); then echo "FAILED modules: ${failed[*]}"; exit 1; fi
echo "All solutions pass."
