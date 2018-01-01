# Features

- WHIP
- CF
- Pausable (requests are queued for later processing, else sync call.)
- 500 error if upstreams not available. "[HTTP] Connection to WHIP server is not established. Sending HTTP 500"
- 

# URL

domain/path?query

			// /CAPS/HTT/ADDCAP/{TOKEN}/{CAP_UUID}/{BANDWIDTH}
			// /CAPS/HTT/REMCAP/{TOKEN}/{CAP_UUID}
			// /CAPS/HTT/PAUSE/{TOKEN}/{CAP_UUID}
			// /CAPS/HTT/RESUME/{TOKEN}/{CAP_UUID}
			// /CAPS/HTT/LIMIT/{TOKEN}/{CAP_UUID}/{BANDWIDTH}
			// /CAPS/HTT/{CAP_UUID}/?texture_id={TEXTUREUUID}
			// /CAPS/HTT/{CAP_UUID}/?mesh_id={TEXTUREUUID}



## Asset request

path == 
			// /CAPS/HTT/{CAP_UUID}/?texture_id={TEXTUREUUID}
			// /CAPS/HTT/{CAP_UUID}/?mesh_id={TEXTUREUUID}

query is standard keypairs.

Query keys, one required. If both, texture_id is used:
- texture_id = guid
- mesh_id = guid

### Request Headers

- `Range`
	- value is `bytes=a-b` where `a` and `b` are nullable integers specifying the byte range to return.  IE: `-123` is from start to byte 123 inclusive, whereas `123-` is from byte 123 inclusive to end.

### Responses

If the asset requested is NOT a texture or mesh, reply with 404.

Texture MIME: `image/x-j2c`
Mesh MIME: `application/vnd.ll.mesh`
TODO: add support for the other three image types.

If there's a range error/exception sending the data to the client, send a `RANGE_ERROR` with at Content-Range header of `bytes 0-{max}/{max}`, other errors are `BAD_REQUEST`

If a `Range` header was sent, only send the bytes in that range.  If the range exceeds bounardies of data, clamp to the the data boundaries.  If the range is backwards, just return all the data.

The data sent is just the internal asset data: if a JPEG, just the JPEG, none of the asset headers.

Response headers

- `Content-Length` with integer value of the full asset size in bytes.
- `Content-Type` with string of MIME type.
- `Content-Range`
	- Only if `Range` header was sent.
	- `bytes a-b/c` where `a` is the start of the range being sent, `b` is the end of the range being sent, and `c` is the total number of bytes of the full asset data - the same number of bytes that would have been sent if the range had not been specified.

## Add cap

path == 0/1/2/3/4/5/6
			// /CAPS/HTT/ADDCAP/{TOKEN}/{CAP_UUID}/{BANDWIDTH}

0. ?
1. ?
2. ?
3. ?
4. cap token, should match token set in config.
5. CAP ID
6. bandwidth in bytes/sec to use for the cap. if it is missing there is no limit

## Remove cap

path == 0/1/2/3/4/5
			// /CAPS/HTT/REMCAP/{TOKEN}/{CAP_UUID}

0. ?
1. ?
2. ?
3. ?
4. cap token, should match token set in config.
5. CAP ID

## Pause cap

path == 0/1/2/3/4/5
			// /CAPS/HTT/PAUSE/{TOKEN}/{CAP_UUID}

0. ?
1. ?
2. ?
3. ?
4. cap token, should match token set in config.
5. CAP ID

## Resume cap

path == 0/1/2/3/4/5
			// /CAPS/HTT/RESUME/{TOKEN}/{CAP_UUID}

0. ?
1. ?
2. ?
3. ?
4. cap token, should match token set in config.
5. CAP ID

Immediately and synchronously dequeues all pending requests in this cap.

## Limit cap

path == 0/1/2/3/4/5/6
			// /CAPS/HTT/LIMIT/{TOKEN}/{CAP_UUID}/{BANDWIDTH}

0. ?
1. ?
2. ?
3. ?
4. cap token, should match token set in config.
5. CAP ID
6. bandwidth in bytes/sec to use for the cap. if it is missing there is no limit

            //maximum bandwidth the viewer provides to us with the slider
            const int VIEWER_BW_MAX = 319000;
            if (bwLimit >= VIEWER_BW_MAX)
            {
                const int REPLACEMENT_BW = 625000;

                //if the user is asking for the max bandwidth, set the
                //texture bandwidth to at least 5 mbit instead of 2.5
                bwLimit = std::max(REPLACEMENT_BW, bwLimit);
            }

