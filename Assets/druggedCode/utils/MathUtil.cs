using UnityEngine;
using UnityEngine.Events;

namespace druggedcode
{
    public class MathUtil
    {
        static public float GetRandom(float min = 0.0f , float max = 1.0f )
        {
            return Random.Range( min , max );
        }

        static public int GetRandomInt(int min = 0 , int max = 1 )
        {
            return Random.Range( min , max );
        }

        static public bool GetRandomBool( float successPer = 0.5f )
        {
            if( GetRandom() < successPer )
                return true;
            else
                return false;
        }

        static public void RandomExecute( params UnityAction[] funcs )
        {

            int count = funcs.Length;
            float quotient = 1 / (float)count;
            float ran = MathUtil.GetRandom();

            while( count-- > 0 )
            {
                if( ran > quotient  * count )
                {
                    funcs[ count ]();
                    break;
                }
            }
//            float ran = MathUtil.GetRandom();
//            
//            if( ran < 0.33f )
//            {
//                FastReady();
//            }
//            else if( ran < 0.66f )
//            {
//                DelayReady();
//            }else
//            {
//                BlitzReady();
//            }
        }

        static public float ByteToMB( float fromByte, uint fix = 1)
        {
            return fromByte * 0.000000954f;
        }

        /// <summary>
        /// Round the specified num and roundInterval.
        /// 소수점 둘째자리로 반올림 - MathUtil.round(12.3234,0.01) //12.32
        /// 4의곱에 가깝게 반올림 - MathUtil.round(22.1,4) // 24
        /// 0.5 단위로 반올림  - MathUtil.round(24.271,0.5) // 24.5 
        /// </summary>
        static public float Round( float num, float roundInterval )
        {
            return Mathf.Round( num/ roundInterval) * roundInterval;
        }

        //최소값과 최대값을 입력 후 범위안 임의의 값을 입력하면 해당 퍼센테이지를 반환 0~1
        //최소값과 최대값을 입력 후 범위안 임의의 값을 입력하면 해당 퍼센테이지를 반환 0~1
        static public float GetRatePer( float current, float max )
        {
            return GetRatePer( 0f, max, current );
        }

        static public float GetRatePer( float min, float max, float value )
        {
            return Mathf.InverseLerp( min, max, value );

            /*
            float per;

            if( value < min )
                value = min;
            else if( value > max )
                value = max;

            float range = max - min;
            float current = value - min;

            if( current == 0 )
                per = 0f;
            else
            {
                per = current/range;
                per = Round( per, 0.001f );
            }

            return per;
            */
        }

        //최소값과 최대값을 입력후 퍼센테이지를 입력하면 해당하는 범위안의 값을 반환. 퍼센테이지는 0~1
        static public float GetRateValue( float min, float max, float per )
        {
            return Mathf.Lerp( min, max, per );

            /*
            if( per < 0f )
                per = 0f;
            else if( per > 1f )
                per = 1f;

            float range = max - min;
            float current = range * per;
            float value = current + min;
            return value;
            */
        }

        /*
        }
        //각도를 -360 ~ 360 사이의 값으로 변환 (500 -> 140,  -400 -> -40)
        public static function degreeParsing($degree:Number):Number
        {
            if (Math.abs($degree)<360)
                return $degree;
            else
            {
                if( $degree > 0 )
                    return $degree - 360;
                else
                    return $degree + 360;
            }
            
            return $degree;
        }
        
        //각도를 0~360 사이의 값으로 변환
        public static function degreeParsingUint( $degree: Number): Number
        {
            while ($degree < 0) {
                $degree += 360;
            }
            while ($degree >= 360) {
                $degree -= 360;
            }
            return $degree;
        }
        

        //360도를 $seg 로 분할 하여 $degree 의 index를 구한다.<br/>순서는 시계방향.<br/>
        // findAngleIndex( 0,4 ) > 0
        //findAngleIndex( 45,4 ) > 1
        //<code>findAngleIndex( 60,4 )</code> is 1
        //<code>findAngleIndex( 120,4 )</code> is 1
        //code>findAngleIndex( 135,4 )</code> is 2
        //<code>findAngleIndex( 280,4 )</code> is 3

        static public function findAngleIndex( $degree:Number, $seg:int = 4 ) :int
        {
            var margin: Number = 360/$seg;
            $degree = degreeParsingUint($degree + margin/2);
            return Math.floor($degree / margin);
        }
*/
    }
}


