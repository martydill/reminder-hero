#!/usr/bin/python

import httplib, urllib
import sys
import email
import time

tries = 5
time_factor = 5
host = 'staging.example.com'
#host = 'requestb.in'
url = '/Home/ParseEmail'
#url = '/ny6fxfny'

lines = sys.stdin.readlines()

full_msg = "".join(lines)
full_msg = full_msg.replace("\n", "\r\n")

values = {
	'message': full_msg
}

headers = {
    'User-Agent': 'python',
    'Content-Type': 'application/x-www-form-urlencoded',
}

values = urllib.urlencode(values)
for i in range(1, tries + 1):
	try:
		conn = httplib.HTTPConnection(host)
		conn.request("POST", url, values, headers)
		response = conn.getresponse()
		break
	except:
		time.sleep(i * time_factor)


