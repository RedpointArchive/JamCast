#!/usr/bin/env groovy

stage("Windows") {
  node('windows') {
    checkout poll: false, changelog: false, scm: scm
    bat ("Protobuild.exe --upgrade-all")
    bat ('Protobuild.exe --automated-build')
    archiveArtifacts artifacts: 'Bundle.Windows.zip', fingerprint: true
  }
}

stage("Mac") {
  node('mac') {
    checkout poll: false, changelog: false, scm: scm
    sh ("/usr/local/bin/mono Protobuild.exe --upgrade-all")
    sh ("/usr/local/bin/mono Protobuild.exe --automated-build")
    archiveArtifacts artifacts: 'Bundle.MacOS.zip', fingerprint: true
  }
}