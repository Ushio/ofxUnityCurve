#include "ofApp.h"

//--------------------------------------------------------------
void ofApp::setup(){
	_simpleCurve.load("SimpleCurve.json");
	_simpleCurve.printKeys();

	_sphereAnimationCurve.load("SphereAnimation.json");
	_sphereAnimationCurve.printKeys();

	_camera.setNearClip(0.01f);
	_camera.setFarClip(500.0f);
	_camera.setDistance(5.0f);

}

//--------------------------------------------------------------
void ofApp::update(){

}

//--------------------------------------------------------------
void ofApp::draw(){
	ofClear(0);
	_camera.begin();
	ofPushMatrix();
	ofRotateZ(90.0f);
	ofSetColor(64);
	ofDrawGridPlane(1, 10);
	ofPopMatrix();

	ofDrawAxis(5.0f);

	ofSetColor(255);

	float e = ofGetElapsedTimef();

	float time01 = fmodf(e, _sphereAnimationCurve.maxDuration());
	ofVec3f sphere(
		_sphereAnimationCurve.evaluate("/m_LocalPosition.x", time01),
		_sphereAnimationCurve.evaluate("/m_LocalPosition.y", time01),
		_sphereAnimationCurve.evaluate("/m_LocalPosition.z", time01)
	);
	ofDrawSphere(sphere, 0.1f);

	ofPolyline line;
	for (int i = 0; i < 100; ++i) {
		float x = ofMap(i, 0, 100, -0.5, 1.5);
		float y = _simpleCurve.evaluate("curve_a", x);
		line.addVertex(x, y);
	}
	line.draw();

	_camera.end();
}

//--------------------------------------------------------------
void ofApp::keyPressed(int key){

}

//--------------------------------------------------------------
void ofApp::keyReleased(int key){

}

//--------------------------------------------------------------
void ofApp::mouseMoved(int x, int y ){

}

//--------------------------------------------------------------
void ofApp::mouseDragged(int x, int y, int button){

}

//--------------------------------------------------------------
void ofApp::mousePressed(int x, int y, int button){

}

//--------------------------------------------------------------
void ofApp::mouseReleased(int x, int y, int button){

}

//--------------------------------------------------------------
void ofApp::mouseEntered(int x, int y){

}

//--------------------------------------------------------------
void ofApp::mouseExited(int x, int y){

}

//--------------------------------------------------------------
void ofApp::windowResized(int w, int h){

}

//--------------------------------------------------------------
void ofApp::gotMessage(ofMessage msg){

}

//--------------------------------------------------------------
void ofApp::dragEvent(ofDragInfo dragInfo){ 

}
