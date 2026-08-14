import re, base64, sys, os

lic = os.environ['UNITY_LICENSE']
m = re.search(r'<DeveloperData Value="([^"]*)"', lic)
if not m:
    sys.exit('serial not found in UNITY_LICENSE')
raw = m.group(1)
try:
    dec = base64.b64decode(raw)
    s = dec.decode('utf-8', 'replace')
    m2 = re.search(r'[A-Z0-9]{2,}(?:-[A-Z0-9]{4,})+', s)
    if m2:
        s = m2.group(0)
except Exception:
    s = raw
if not re.match(r'^[A-Z0-9-]{20,30}$', s):
    sys.exit('unusable serial: ' + s[:8])
print(f'UNITY_SERIAL={s}')