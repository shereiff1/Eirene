import { View, Text } from 'react-native'
import React from 'react'
import { useGlobalSearchParams } from 'expo-router'

const PostDetails = () => {

    const {id} = useGlobalSearchParams();
    

  return (
    <View>
      <Text>[post_id]</Text>
    </View>
  )
}

export default PostDetails