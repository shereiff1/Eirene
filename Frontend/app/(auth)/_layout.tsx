import { View, Text } from 'react-native'
import React from 'react'
import { Tabs } from 'expo-router'

const _layout = () => {
  return (
    <Tabs>
      <Tabs.Screen 
        name='sign_in' 
        options={{
          title: "Sign in",
          headerShown: false
        }}
      />
      <Tabs.Screen
       name='sign_up'
       options={{
        title: "Sign up",
        headerShown: false
       }} 
      />
    </Tabs>
  )
}

export default _layout