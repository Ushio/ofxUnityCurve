#pragma once

#include "ofMain.h"
#include <map>
#include <vector>
#include <algorithm>
#include "rapidjson/rapidjson.h"
#include "rapidjson/document.h"

class ofxUnityCurve {
public:
	struct Keyframe {
		float value = 0.0f;
		float time = 0.0f;
		float inTangent = 0.0f;
		float outTangent = 0.0f;
	};

	void load(const char *name) {
		_curveName = name;

		ofBuffer jsonBuffer = ofBufferFromFile(name, false);

		rapidjson::Document document;
		document.ParseInsitu(jsonBuffer.getData());
		if (document.HasParseError()) {
			printf("failed to load curves\n");
			return;
		}

		const auto &curves = document.GetArray();
		std::map<std::string, std::vector<Keyframe>> newCurves;
		for (int i = 0; i < curves.Size(); ++i) {
			const auto &curve = curves[i];
			const auto &name = curve["property"];
			const auto &keys = curve["keys"];

			std::vector<Keyframe> keyframes(keys.Size());
			if (keyframes.empty()) {
				continue;
			}

			for (int j = 0; j < keys.Size(); ++j) {
				const auto &key = keys[j];
				keyframes[j].value = key["value"].GetFloat();
				keyframes[j].time = key["time"].GetFloat();
				keyframes[j].inTangent = key["inTangent"].GetFloat();
				keyframes[j].outTangent = key["outTangent"].GetFloat();
			}
			newCurves[name.GetString()] = keyframes;
		}
		
		_curves = newCurves;
	}

	float evaluate(const char *key, float time) const {
		auto it = _curves.find(key);
		if (it == _curves.end()) {
			printf("UnityCurve warning: \"%s\" not found.\n", key);
			return 0.0f;
		}

		return _Evaluate(it->second, time);
	}

	std::vector<std::string> keys() const {
		std::vector<std::string> r;
		for (auto it = _curves.begin(); it != _curves.end(); ++it)
		{
			r.push_back(it->first);
		}
		return r;
	}

	void printKeys() const {
		printf("UnityCurve -key-\n");
		for (auto it = _curves.begin(); it != _curves.end(); ++it)
		{
			printf("%s\n", it->first.c_str());
		}
	}
	void reload() {
		load(_curveName.c_str());
	}

	float duration(const char *key) const {
		auto it = _curves.find(key);
		if (it == _curves.end()) {
			printf("UnityCurve warning: \"%s\" not found.\n", key);
			return 0.0f;
		}
		return it->second[it->second.size() - 1].time;
	}
	float maxDuration() const {
		float d = 0.0f;
		for (auto it = _curves.begin(); it != _curves.end(); ++it)
		{
			if (!it->second.empty()) {
				d = std::max(d, it->second[it->second.size() - 1].time);
			}
		}
		return d;
	}
private:
	float _Evaluate(Keyframe keyframe0, Keyframe keyframe1, float t) const {
		float dt = keyframe1.time - keyframe0.time;

		float m0 = keyframe0.outTangent * dt;
		float m1 = keyframe1.inTangent * dt;

		float t2 = t * t;
		float t3 = t2 * t;

		float a = 2 * t3 - 3 * t2 + 1;
		float b = t3 - 2 * t2 + t;
		float c = t3 - t2;
		float d = -2 * t3 + 3 * t2;

		return a * keyframe0.value + b * m0 + c * m1 + d * keyframe1.value;
	}
	float _Evaluate(const std::vector<Keyframe> &keys, float time) const {
		if (keys.size() == 0)
		{
			return 0.0f;
		}
		if (keys.size() == 1)
		{
			return keys[0].value;
		}

		int upper = (int)keys.size() - 1;
		int lower = 0;

		if (time <= keys[lower].time)
		{
			return keys[lower].value;
		}
		if (keys[upper].time <= time)
		{
			return keys[upper].value;
		}

		while (lower + 1 != upper)
		{
			int mid = (lower + upper) / 2;
			if (time < keys[mid].time)
			{
				upper = mid;
			}
			else {
				lower = mid;
			}
		}
		Keyframe keyframe0 = keys[lower];
		Keyframe keyframe1 = keys[upper];
		float t = (time - keyframe0.time) / (keyframe1.time - keyframe0.time);
		return _Evaluate(keyframe0, keyframe1, t);
	}

	std::string _curveName;
	std::map<std::string, std::vector<Keyframe>> _curves;
};